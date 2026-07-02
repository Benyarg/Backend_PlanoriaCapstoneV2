using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Cronograma.Requests;
using PlanoriaCapstone.DTOs.Cronograma.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _context;
    private readonly IStudyScheduleRepository _scheduleRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IFlashcardDeckRepository _deckRepository;
    private readonly IQuizRepository _quizRepository;

    public ScheduleService(
    IStudyScheduleRepository scheduleRepository,
    ICourseRepository courseRepository,
    IActivityLogRepository activityLogRepository,
    IFlashcardDeckRepository deckRepository,
    IQuizRepository quizRepository,
    AppDbContext context)
    {
        _scheduleRepository = scheduleRepository;
        _courseRepository = courseRepository;
        _activityLogRepository = activityLogRepository;
        _deckRepository = deckRepository;
        _quizRepository = quizRepository;
        _context = context;
    }

    // ============================================
    // CRUD
    // ============================================

    public async Task<ScheduleResponseDto> GetByIdAsync(int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
            throw new KeyNotFoundException($"Schedule with id {id} not found");

        return MapToResponseDto(schedule);
    }

    public async Task<IEnumerable<ScheduleListResponseDto>> GetByUserAsync(int userId)
    {
        var schedules = await _scheduleRepository.GetByUserAsync(userId);
        return await MapToListAsync(schedules);
    }
    public async Task<IEnumerable<StudySchedule>> GetByUserBasicAsync(int userId)
    {
        return await _scheduleRepository.GetByUserAsync(userId);
    }

    public async Task<IEnumerable<ScheduleListResponseDto>> GetByDateRangeAsync(int userId, DateTime from, DateTime to)
    {
        var schedules = await _scheduleRepository.GetByDateRangeAsync(userId, from, to);
        return await MapToListAsync(schedules);
    }

    public async Task<ScheduleResponseDto> CreateAsync(int userId, CreateScheduleRequestDto request)
    {
        var schedule = new StudySchedule
        {
            UserId = userId,
            Title = request.Title,
            StartDatetime = request.StartDateTime,
            EndDatetime = request.EndDateTime,
            IsCompleted = false,
            NotificationSent = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _scheduleRepository.CreateAsync(schedule);

        if (request.CourseIds != null)
        {
            foreach (var courseId in request.CourseIds)
            {
                await _scheduleRepository.AddContentAsync(new ScheduleContent
                {
                    ScheduleId = created.Id,
                    ContentType = "Course",
                    ContentId = courseId
                });
            }
        }

        if (request.Intervals != null)
        {
            foreach (var interval in request.Intervals)
            {
                await _scheduleRepository.AddIntervalAsync(new ScheduleInterval
                {
                    ScheduleId = created.Id,
                    IntervalType = interval.IntervalType,
                    DurationMinutes = interval.DurationMinutes,
                    OrderPosition = interval.OrderPosition
                });
            }
        }

        if (request.Content != null && request.CourseIds != null && request.CourseIds.Any())
        {
            foreach (var content in request.Content)
            {
                if (await ValidateContentBelongsToCourse(content, request.CourseIds))
                {
                    await _scheduleRepository.AddContentAsync(new ScheduleContent
                    {
                        ScheduleId = created.Id,
                        ContentType = content.ContentType,
                        ContentId = content.ContentId,
                        EstimatedMinutes = content.EstimatedMinutes > 0 ? content.EstimatedMinutes : null,
                        Completed = false
                    });
                }
            }
        }

        await LogAsync(userId, "CreateSchedule", "StudySchedule", created.Id);
        return await GetByIdAsync(created.Id);
    }

    public async Task<ScheduleResponseDto> UpdateAsync(int id, UpdateScheduleRequestDto request)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
            throw new KeyNotFoundException($"Schedule with id {id} not found");

        if (request.Title != null) schedule.Title = request.Title;
        if (request.StartDateTime.HasValue) schedule.StartDatetime = request.StartDateTime.Value;
        if (request.EndDateTime.HasValue) schedule.EndDatetime = request.EndDateTime.Value;
        if (request.IsCompleted.HasValue) schedule.IsCompleted = request.IsCompleted.Value;
        schedule.UpdatedAt = DateTime.UtcNow;

        var updated = await _scheduleRepository.UpdateAsync(schedule);
        return MapToResponseDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
        => await _scheduleRepository.DeleteAsync(id);

    // ============================================
    // VISTAS DE CALENDARIO
    // ============================================

    public async Task<object> GetMonthViewAsync(int userId, int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var schedules = await _scheduleRepository.GetByDateRangeAsync(userId, start, end);
        var list = await MapToListAsync(schedules);

        return new
        {
            Year = year,
            Month = month,
            Days = Enumerable.Range(1, end.Day).Select(day => new
            {
                Date = new DateTime(year, month, day),
                Schedules = list.Where(s => s.StartDateTime.Date <= new DateTime(year, month, day) && s.EndDateTime.Date >= new DateTime(year, month, day))
            })
        };
    }

    public async Task<object> GetWeekViewAsync(int userId, int year, int week)
    {
        var start = new DateTime(year, 1, 1).AddDays((week - 1) * 7);
        var end = start.AddDays(7);
        var schedules = await _scheduleRepository.GetByDateRangeAsync(userId, start, end);
        var list = await MapToListAsync(schedules);

        return new CalendarWeekResponseDto
        {
            WeekStart = start,
            WeekEnd = end,
            Days = Enumerable.Range(0, 7).Select(i => new CalendarDayResponseDto
            {
                Date = start.AddDays(i),
                Schedules = list.Where(s => s.StartDateTime.Date <= start.AddDays(i) && s.EndDateTime.Date >= start.AddDays(i)).ToList(),
                TotalStudyMinutes = 0,
                CompletedSessionsCount = schedules.Count(s => s.IsCompleted)
            }).ToList()
        };
    }

    public async Task<object> GetDayViewAsync(int userId, DateTime date)
    {
        var schedules = await _scheduleRepository.GetByDateRangeAsync(userId, date.Date, date.Date.AddDays(1));
        var list = await MapToListAsync(schedules);

        return new CalendarDayResponseDto
        {
            Date = date,
            Schedules = list.ToList(),
            TotalStudyMinutes = (int)schedules.Sum(s => (s.EndDatetime - s.StartDatetime).TotalMinutes),
            CompletedSessionsCount = schedules.Count(s => s.IsCompleted)
        };
    }

    public async Task<object> GetAgendaAsync(int userId, DateTime from, DateTime to)
    {
        var schedules = await _scheduleRepository.GetByDateRangeAsync(userId, from, to);
        return schedules.OrderBy(s => s.StartDatetime).Select(s => new
        {
            s.Id,
            s.Title,
            StartDateTime = s.StartDatetime,
            EndDateTime = s.EndDatetime,
            s.IsCompleted,
            DurationMinutes = (int)(s.EndDatetime - s.StartDatetime).TotalMinutes
        });
    }

    // ============================================
    // RECURRING
    // ============================================

    public async Task CreateRecurringAsync(int userId, CreateScheduleRequestDto request, string recurrence)
    {
        var current = request.StartDateTime;
        var count = recurrence.ToLower() switch { "daily" => 7, "weekly" => 4, "biweekly" => 2, "monthly" => 3, _ => 1 };

        for (int i = 0; i < count; i++)
        {
            await CreateAsync(userId, new CreateScheduleRequestDto
            {
                Title = request.Title,
                StartDateTime = current,
                EndDateTime = current.Add(request.EndDateTime - request.StartDateTime),
                CourseIds = request.CourseIds,
                Intervals = request.Intervals,
                Content = request.Content
            });

            current = recurrence.ToLower() switch
            {
                "daily" => current.AddDays(1),
                "weekly" => current.AddDays(7),
                "biweekly" => current.AddDays(14),
                "monthly" => current.AddMonths(1),
                _ => current
            };
        }
    }

    public Task UpdateRecurringAsync(int id, UpdateScheduleRequestDto r) => UpdateAsync(id, r);
    public Task DeleteRecurringAsync(int id) => DeleteAsync(id);

    // ============================================
    // COMPLETAR
    // ============================================

    public async Task MarkCompleteAsync(int scheduleId)
    {
        var s = await _scheduleRepository.GetByIdAsync(scheduleId) ?? throw new KeyNotFoundException();
        s.IsCompleted = true;
        s.CompletedAt = DateTime.UtcNow;
        s.UpdatedAt = DateTime.UtcNow;
        await _scheduleRepository.UpdateAsync(s);
    }

    public async Task MarkIncompleteAsync(int scheduleId)
    {
        var s = await _scheduleRepository.GetByIdAsync(scheduleId) ?? throw new KeyNotFoundException();
        s.IsCompleted = false;
        s.CompletedAt = null;
        s.UpdatedAt = DateTime.UtcNow;
        await _scheduleRepository.UpdateAsync(s);
    }

    public async Task BulkCompleteAsync(List<int> ids)
    {
        foreach (var id in ids) await MarkCompleteAsync(id);
    }

    // ============================================
    // VALIDACI�N
    // ============================================

    private async Task<bool> ValidateContentBelongsToCourse(ScheduleContentRequestDto content, List<int> courseIds)
    {
        if (content.ContentType == "flashcard_deck")
        {
            var deck = await _deckRepository.GetByIdAsync(content.ContentId);
            return deck != null && courseIds.Contains(deck.CourseId);
        }
        if (content.ContentType == "quiz")
        {
            var quiz = await _quizRepository.GetByIdAsync(content.ContentId);
            return quiz != null && courseIds.Contains(quiz.CourseId);
        }
        return false;
    }

    // ============================================
    // MAPEO
    // ============================================

    private async Task<List<ScheduleListResponseDto>> MapToListAsync(IEnumerable<StudySchedule> schedules)
    {
        var scheduleList = schedules.ToList();
        if (!scheduleList.Any()) return new List<ScheduleListResponseDto>();

        var scheduleIds = scheduleList.Select(s => s.Id).ToList();

        var courseMappings = await _context.ScheduleContents
            .Where(c => scheduleIds.Contains(c.ScheduleId) && c.ContentType == "Course")
            .ToListAsync();

        var courseIds = courseMappings.Select(c => c.ContentId).Distinct().ToList();
        var courses = await _context.Courses
            .Where(c => courseIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        var allContents = scheduleList
            .SelectMany(s => s.ScheduleContents ?? new List<ScheduleContent>())
            .ToList();

        var quizIds = allContents.Where(c => c.ContentType == "quiz").Select(c => c.ContentId).Distinct().ToList();
        var deckIds = allContents.Where(c => c.ContentType == "flashcard_deck").Select(c => c.ContentId).Distinct().ToList();

        var completedQuizIds = new HashSet<int>();
        if (quizIds.Count > 0)
        {
            var attempted = await _context.QuizAttempts
                .Where(a => quizIds.Contains(a.QuizId) && a.CompletedAt != null)
                .Select(a => a.QuizId)
                .Distinct()
                .ToListAsync();
            completedQuizIds = attempted.ToHashSet();
        }

        var flashcardProgressByDeck = new Dictionary<int, UserProgressFlashcard>();
        var deckTotalCardsMap = new Dictionary<int, int>();
        if (deckIds.Count > 0)
        {
            var progressRecords = await _context.UserProgressFlashcards
                .Where(p => deckIds.Contains(p.DeckId))
                .ToListAsync();
            foreach (var p in progressRecords)
                flashcardProgressByDeck[p.DeckId] = p;

            var decks = await _context.FlashcardDecks
                .Where(d => deckIds.Contains(d.Id))
                .ToListAsync();
            foreach (var d in decks)
                deckTotalCardsMap[d.Id] = d.TotalCards;
        }

        return scheduleList.Select(s =>
        {
            var mapping = courseMappings.FirstOrDefault(m => m.ScheduleId == s.Id);
            var name = "";
            var color = "#3498db";
            if (mapping != null && courses.TryGetValue(mapping.ContentId, out var course))
            {
                name = course.Name;
                color = course.ColorHex;
            }

            var contents = s.ScheduleContents?.Where(c => c.ContentType != "Course").ToList() ?? new List<ScheduleContent>();
            var totalContent = contents.Count;

            decimal progress;
            if (s.IsCompleted)
            {
                progress = 100;
            }
            else if (totalContent == 0)
            {
                progress = 0;
            }
            else
            {
                var weightedSum = 0m;
                foreach (var c in contents)
                {
                    if (c.Completed)
                    {
                        weightedSum += 100;
                    }
                    else if (c.ContentType == "quiz")
                    {
                        weightedSum += completedQuizIds.Contains(c.ContentId) ? 100 : 0;
                    }
                    else if (c.ContentType == "flashcard_deck")
                    {
                        var fp = flashcardProgressByDeck.GetValueOrDefault(c.ContentId);
                        var total = deckTotalCardsMap.GetValueOrDefault(c.ContentId);
                        if (total > 0 && fp != null)
                            weightedSum += Math.Round((decimal)fp.CardsMastered / total * 100, 1);
                        else
                            weightedSum += 0;
                    }
                    else
                    {
                        weightedSum += 0;
                    }
                }
                progress = Math.Round(weightedSum / totalContent, 1);
            }

            return new ScheduleListResponseDto
            {
                Id = s.Id,
                Title = s.Title,
                StartDateTime = s.StartDatetime,
                EndDateTime = s.EndDatetime,
                IsCompleted = s.IsCompleted,
                ProgressPercentage = progress,
                CourseName = name,
                ColorHex = color
            };
        }).ToList();
    }

    private ScheduleResponseDto MapToResponseDto(StudySchedule s)
    {
        return new ScheduleResponseDto
        {
            Id = s.Id,
            UserId = s.UserId,
            Title = s.Title,
            StartDateTime = s.StartDatetime,
            EndDateTime = s.EndDatetime,
            IsCompleted = s.IsCompleted,
            CompletedAt = s.CompletedAt,
            TotalDurationMinutes = (int)(s.EndDatetime - s.StartDatetime).TotalMinutes,
            CourseIds = s.ScheduleContents?.Where(c => c.ContentType == "Course").Select(c => c.ContentId).ToList() ?? new List<int>(),
            Intervals = s.ScheduleIntervals?.Select(i => new IntervalResponseDto
            {
                Id = i.Id,
                IntervalType = i.IntervalType,
                DurationMinutes = i.DurationMinutes,
                OrderPosition = i.OrderPosition,
                StartedAt = i.StartedAt,
                EndedAt = i.EndedAt,
                IsCompleted = i.EndedAt.HasValue
            }).ToList() ?? new List<IntervalResponseDto>(),
            Content = s.ScheduleContents?.Where(c => c.ContentType != "Course").Select(c => new ScheduleContentResponseDto
            {
                Id = c.Id,
                ContentType = c.ContentType,
                ContentId = c.ContentId,
                ContentName = GetContentName(c),
                EstimatedMinutes = c.EstimatedMinutes ?? 0,
                Completed = c.Completed,
                CompletedAt = c.CompletedAt
            }).ToList() ?? new List<ScheduleContentResponseDto>()
        };
    }

    private string GetContentName(ScheduleContent c)
    {
        if (c.ContentType == "flashcard_deck")
            return _deckRepository.GetByIdAsync(c.ContentId).Result?.Name ?? $"Deck #{c.ContentId}";
        if (c.ContentType == "quiz")
            return _quizRepository.GetByIdAsync(c.ContentId).Result?.Title ?? $"Quiz #{c.ContentId}";
        return "";
    }

    private async Task LogAsync(int userId, string action, string entity, int? entityId)
    {
        try
        {
            await _activityLogRepository.LogAsync(new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entity,
                EntityId = entityId,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch { }
    }
}