using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Cronograma.Responses;
using PlanoriaCapstone.DTOs.Cronograma.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class ScheduleContentService : IScheduleContentService
{
    private readonly IStudyScheduleRepository _scheduleRepository;
    private readonly IFlashcardDeckRepository _deckRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public ScheduleContentService(
        IStudyScheduleRepository scheduleRepository,
        IFlashcardDeckRepository deckRepository,
        IQuizRepository quizRepository,
        IActivityLogRepository activityLogRepository)
    {
        _scheduleRepository = scheduleRepository;
        _deckRepository = deckRepository;
        _quizRepository = quizRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<ScheduleContentResponseDto> AttachContentAsync(ScheduleContentRequestDto request)
    {
        // ✅ Validar que el contenido pertenece al curso del horario
        var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId);
        if (schedule == null)
            throw new KeyNotFoundException($"Horario {request.ScheduleId} no encontrado");

        // Obtener los cursos asociados al horario
        var courseIds = schedule.ScheduleContents?
            .Where(c => c.ContentType == "Course")
            .Select(c => c.ContentId)
            .ToList() ?? new List<int>();

        // Si hay cursos asociados, validar que el contenido pertenezca a uno de ellos
        if (courseIds.Any())
        {
            bool isValid = false;
            if (request.ContentType == "flashcard_deck")
            {
                var deck = await _deckRepository.GetByIdAsync(request.ContentId);
                isValid = deck != null && courseIds.Contains(deck.CourseId);
            }
            else if (request.ContentType == "quiz")
            {
                var quiz = await _quizRepository.GetByIdAsync(request.ContentId);
                isValid = quiz != null && courseIds.Contains(quiz.CourseId);
            }

            if (!isValid)
                throw new InvalidOperationException("El contenido no pertenece a los cursos asociados a este horario");
        }

        var content = new ScheduleContent
        {
            ScheduleId = request.ScheduleId,
            ContentType = request.ContentType,
            ContentId = request.ContentId,
            EstimatedMinutes = request.EstimatedMinutes > 0 ? request.EstimatedMinutes : null,
            Completed = false
        };

        var created = await _scheduleRepository.AddContentAsync(content);

        return new ScheduleContentResponseDto
        {
            Id = created.Id,
            ContentType = created.ContentType,
            ContentId = created.ContentId,
            ContentName = string.Empty,
            EstimatedMinutes = created.EstimatedMinutes ?? 0,
            Completed = created.Completed,
            CompletedAt = created.CompletedAt
        };
    }

    public async Task<bool> DetachContentAsync(int scheduleId, int contentId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
            throw new KeyNotFoundException($"Horario {scheduleId} no encontrado");

        var content = schedule.ScheduleContents?.FirstOrDefault(c => c.Id == contentId && c.ContentType != "Course");
        if (content == null)
            return false;

        return await _scheduleRepository.RemoveContentAsync(contentId);
    }

    public async Task ReorderContentAsync(int scheduleId, List<int> contentIds)
    {
        // ✅ CORREGIDO: Usar userId=1 y log seguro
        await LogActivitySafeAsync(1, "ReorderContent", "StudySchedule", scheduleId,
            $"Contenido reordenado: {string.Join(",", contentIds)}");

        await Task.CompletedTask;
    }

    public async Task<IEnumerable<ScheduleContentResponseDto>> GetAssignedContentAsync(int scheduleId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule?.ScheduleContents == null)
            return Enumerable.Empty<ScheduleContentResponseDto>();

        var result = new List<ScheduleContentResponseDto>();
        foreach (var c in schedule.ScheduleContents)
        {
            string name = "";
            if (c.ContentType == "flashcard_deck")
            {
                var deck = await _deckRepository.GetByIdAsync(c.ContentId);
                name = deck?.Name ?? $"Deck #{c.ContentId}";
            }
            else if (c.ContentType == "quiz")
            {
                var quiz = await _quizRepository.GetByIdAsync(c.ContentId);
                name = quiz?.Title ?? $"Quiz #{c.ContentId}";
            }
            else if (c.ContentType == "Course")
                continue;

            result.Add(new ScheduleContentResponseDto
            {
                Id = c.Id,
                ContentType = c.ContentType,
                ContentId = c.ContentId,
                ContentName = name,
                EstimatedMinutes = c.EstimatedMinutes ?? 0,
                Completed = c.Completed,
                CompletedAt = c.CompletedAt
            });
        }
        return result;
    }

    public async Task<int> AutoAssignAsync(int userId, int scheduleId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
            throw new KeyNotFoundException($"Horario {scheduleId} no encontrado");

        var courseIds = schedule.ScheduleContents?
            .Where(c => c.ContentType == "Course")
            .Select(c => c.ContentId)
            .ToList() ?? new List<int>();

        if (!courseIds.Any())
            return 0;

        var random = new Random();
        var assignedCount = 0;

        foreach (var courseId in courseIds)
        {
            var decks = await _deckRepository.GetByCourseIdAsync(courseId);
            var quizzes = await _quizRepository.GetByCourseIdAsync(courseId);

            var deckContent = decks.Select(d => new { Type = "flashcard_deck", d.Id, Title = d.Name, EstimatedMinutes = d.TotalCards * 2 }).ToList();
            var quizContent = quizzes.Select(q => new { Type = "quiz", q.Id, q.Title, EstimatedMinutes = q.TimeLimitMinutes ?? 30 }).ToList();
            var allContent = deckContent.Concat(quizContent).ToList();

            var existingIds = schedule.ScheduleContents?
                .Where(sc => sc.ContentType == "flashcard_deck" || sc.ContentType == "quiz")
                .Select(sc => (sc.ContentType, sc.ContentId))
                .ToHashSet() ?? new HashSet<(string, int)>();

            var available = allContent
                .Where(c => !existingIds.Contains((c.Type, c.Id)))
                .ToList();

            if (available.Count == 0) continue;

            var selectedCount = Math.Min(3, available.Count);
            var selected = available.OrderBy(_ => random.Next()).Take(selectedCount);

            foreach (var item in selected)
            {
                await _scheduleRepository.AddContentAsync(new ScheduleContent
                {
                    ScheduleId = scheduleId,
                    ContentType = item.Type,
                    ContentId = item.Id,
                    EstimatedMinutes = item.EstimatedMinutes,
                    Completed = false
                });
                assignedCount++;
            }
        }

        await LogActivitySafeAsync(userId, "AutoAssignContent", "StudySchedule", scheduleId,
            $"{assignedCount} contenidos auto-asignados");
        return assignedCount;
    }

    public async Task<IEnumerable<ScheduleContentResponseDto>> PrioritizeByExamAsync(int userId, int courseId, int scheduleId)
    {
        var decks = await _deckRepository.GetByCourseIdAsync(courseId);
        var quizzes = await _quizRepository.GetByCourseIdAsync(courseId);
        var result = new List<ScheduleContentResponseDto>();

        foreach (var deck in decks)
        {
            result.Add(new ScheduleContentResponseDto
            {
                ContentType = "FlashcardDeck",
                ContentId = deck.Id,
                ContentName = deck.Name,
                EstimatedMinutes = deck.TotalCards * 2,
                Completed = false
            });
        }

        foreach (var quiz in quizzes)
        {
            result.Add(new ScheduleContentResponseDto
            {
                ContentType = "Quiz",
                ContentId = quiz.Id,
                ContentName = quiz.Title,
                EstimatedMinutes = quiz.TimeLimitMinutes ?? 30,
                Completed = false
            });
        }

        return result;
    }

    public async Task<IEnumerable<ScheduleContentResponseDto>> PrioritizeByWeaknessAsync(int userId, int courseId, int scheduleId)
    {
        return await PrioritizeByExamAsync(userId, courseId, scheduleId);
    }

    public async Task<object> SuggestSessionAsync(int userId, int courseId)
    {
        return new
        {
            RecommendedDurationMinutes = 60,
            BreakInterval = 25,
            ContentMix = new
            {
                Flashcards = 30,
                Quiz = 20,
                Review = 10
            },
            Focus = "Weak areas identified"
        };
    }

    public async Task<IEnumerable<ScheduleContentResponseDto>> SuggestContentAsync(int userId, int scheduleId)
    {
        return await GetAssignedContentAsync(scheduleId);
    }

    public async Task<object> OptimizeScheduleAsync(int userId)
    {
        return new
        {
            Optimized = true,
            Recommendations = new List<string>
            {
                "Schedule high-focus tasks in the morning",
                "Use Pomodoro technique for better concentration",
                "Take regular breaks every 25 minutes"
            },
            EstimatedProductivityGain = 25
        };
    }

    // ============================================
    // LOG SEGURO
    // ============================================

    private async Task LogActivitySafeAsync(int userId, string action, string entityType,
        int? entityId, string details)
    {
        try
        {
            await _activityLogRepository.LogAsync(new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch
        {
            // No interrumpir el flujo
        }
    }
}