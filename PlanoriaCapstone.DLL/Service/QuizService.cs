using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Quiz.Requests;
using PlanoriaCapstone.DTOs.Quiz.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public QuizService(
        IQuizRepository quizRepository,
        ICourseRepository courseRepository,
        IActivityLogRepository activityLogRepository)
    {
        _quizRepository = quizRepository;
        _courseRepository = courseRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<QuizResponseDto> GetByIdAsync(int id, int userId)
    {
        var quiz = await _quizRepository.GetByIdAsync(id, userId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {id} no encontrado");

        return MapToResponse(quiz);
    }

    public async Task<IEnumerable<QuizListResponseDto>> GetByCourseIdAsync(int courseId, int userId)
    {
        var quizzes = await _quizRepository.GetByCourseIdAsync(courseId, userId);
        return quizzes.Select(MapToListResponse);
    }

    public async Task<IEnumerable<QuizListResponseDto>> GetAllAsync(int userId)
    {
        var quizzes = await _quizRepository.GetAllAsync(userId);
        return quizzes.Select(MapToListResponse);
    }

    public async Task<QuizResponseDto> CreateAsync(int userId, CreateQuizRequestDto request)
    {
        var quiz = new Quiz
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            PassingScore = request.PassingScore,
            TimeLimitMinutes = request.TimeLimitMinutes,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleOptions = request.ShuffleOptions,
            AttemptsAllowed = request.AttemptsAllowed,
            TotalQuestions = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _quizRepository.CreateAsync(quiz);

        await LogActivitySafeAsync(userId, "CreateQuiz", "Quiz", created.Id,
            $"Quiz '{created.Title}' creado");

        return MapToResponse(created);
    }

    public async Task<QuizResponseDto> UpdateAsync(int id, UpdateQuizRequestDto request)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {id} no encontrado");

        if (request.Title != null) quiz.Title = request.Title;
        if (request.Description != null) quiz.Description = request.Description;
        if (request.PassingScore.HasValue) quiz.PassingScore = request.PassingScore.Value;
        if (request.TimeLimitMinutes.HasValue) quiz.TimeLimitMinutes = request.TimeLimitMinutes;
        if (request.ShuffleQuestions.HasValue) quiz.ShuffleQuestions = request.ShuffleQuestions.Value;
        if (request.ShuffleOptions.HasValue) quiz.ShuffleOptions = request.ShuffleOptions.Value;
        if (request.AttemptsAllowed.HasValue) quiz.AttemptsAllowed = request.AttemptsAllowed.Value;
        if (request.IsActive.HasValue) quiz.IsActive = request.IsActive.Value;
        quiz.UpdatedAt = DateTime.UtcNow;

        var updated = await _quizRepository.UpdateAsync(quiz);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _quizRepository.DeleteAsync(id);
    }

    public async Task<QuizResponseDto> DuplicateAsync(int id, DuplicateQuizRequestDto request)
    {
        var source = await _quizRepository.GetByIdAsync(id);
        if (source == null)
            throw new KeyNotFoundException($"Quiz con ID {id} no encontrado");

        var newQuiz = new Quiz
        {
            CourseId = request.TargetCourseId,
            Title = request.NewTitle ?? $"{source.Title} (Copia)",
            Description = source.Description,
            TotalQuestions = source.TotalQuestions,
            PassingScore = source.PassingScore,
            TimeLimitMinutes = source.TimeLimitMinutes,
            ShuffleQuestions = source.ShuffleQuestions,
            ShuffleOptions = source.ShuffleOptions,
            AttemptsAllowed = source.AttemptsAllowed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            QuizQuestions = source.QuizQuestions?.Select(q => new QuizQuestion
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Explanation = q.Explanation,
                Points = q.Points,
                OrderPosition = q.OrderPosition,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                QuizOptions = q.QuizOptions?.Select(o => new QuizOption
                {
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect,
                    OrderPosition = o.OrderPosition,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            }).ToList()
        };

        var created = await _quizRepository.CreateAsync(newQuiz);
        return MapToResponse(created);
    }

    // ============================================
    // ✅ PREGUNTAS - CORREGIDO
    // ============================================

    public async Task<IEnumerable<QuestionResponseDto>> GetQuestionsAsync(int quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        return (quiz.QuizQuestions ?? new List<QuizQuestion>())
            .OrderBy(q => q.OrderPosition)
            .Select(MapToQuestionResponse);
    }

    public async Task<QuestionResponseDto> CreateQuestionAsync(int quizId, CreateQuestionRequestDto request)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        // Crear pregunta
        var question = new QuizQuestion
        {
            QuizId = quizId,
            QuestionText = request.QuestionText,
            QuestionType = request.QuestionType,
            Explanation = request.Explanation,
            Points = request.Points,
            OrderPosition = request.OrderPosition,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Guardar pregunta
        question = await _quizRepository.AddQuestionAsync(question);

        // Guardar opciones
        if (request.Options != null && request.Options.Any())
        {
            question.QuizOptions = request.Options.Select(o => new QuizOption
            {
                QuestionId = question.Id,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                OrderPosition = o.OrderPosition,
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }

        await _quizRepository.UpdateTotalQuestionsAsync(quizId);

        return MapToQuestionResponse(question);
    }

    public async Task<QuestionResponseDto> UpdateQuestionAsync(int questionId, UpdateQuestionRequestDto request)
    {
        var quiz = await FindQuizByQuestionId(questionId);
        var question = quiz.QuizQuestions!.FirstOrDefault(q => q.Id == questionId);
        if (question == null)
            throw new KeyNotFoundException($"Pregunta con ID {questionId} no encontrada");

        // Actualizar propiedades simples
        if (request.QuestionText != null) question.QuestionText = request.QuestionText;
        if (request.Explanation != null) question.Explanation = request.Explanation;
        if (request.Points.HasValue) question.Points = request.Points.Value;
        if (request.OrderPosition.HasValue) question.OrderPosition = request.OrderPosition.Value;
        //if (request.IsActive.HasValue) question.IsActive = request.IsActive.Value;
        question.UpdatedAt = DateTime.UtcNow;

        // ✅ ACTUALIZAR OPCIONES CORRECTAMENTE
        if (request.Options != null)
        {
            // Obtener opciones existentes
            var existingOptions = question.QuizOptions?.ToList() ?? new List<QuizOption>();

            // Crear lista de IDs de opciones que vienen en la request (con ID > 0)
            var updatedOptionIds = request.Options
                .Where(o => o.Id.HasValue && o.Id.Value > 0)
                .Select(o => o.Id.Value)
                .ToList();

            // Eliminar opciones que ya no están en la request
            var optionsToRemove = existingOptions
                .Where(o => !updatedOptionIds.Contains(o.Id))
                .ToList();

            foreach (var option in optionsToRemove)
            {
                question.QuizOptions.Remove(option);
            }

            // Actualizar opciones existentes o agregar nuevas
            foreach (var optionDto in request.Options)
            {
                if (optionDto.Id.HasValue && optionDto.Id.Value > 0)
                {
                    // Actualizar opción existente
                    var existingOption = existingOptions.FirstOrDefault(o => o.Id == optionDto.Id.Value);
                    if (existingOption != null)
                    {
                        if (optionDto.OptionText != null) existingOption.OptionText = optionDto.OptionText;
                        if (optionDto.IsCorrect.HasValue) existingOption.IsCorrect = optionDto.IsCorrect.Value;
                        if (optionDto.OrderPosition.HasValue) existingOption.OrderPosition = optionDto.OrderPosition.Value;
                    }
                }
                else
                {
                    // Agregar nueva opción
                    var newOption = new QuizOption
                    {
                        QuestionId = questionId,
                        OptionText = optionDto.OptionText ?? string.Empty,
                        IsCorrect = optionDto.IsCorrect ?? false,
                        OrderPosition = optionDto.OrderPosition ?? 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    question.QuizOptions.Add(newOption);
                }
            }
        }

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.UpdateAsync(quiz);

        return MapToQuestionResponse(question);
    }

    public async Task<bool> DeleteQuestionAsync(int questionId)
    {
        var quiz = await FindQuizByQuestionId(questionId);
        if (quiz.QuizQuestions == null) return false;

        var question = quiz.QuizQuestions.FirstOrDefault(q => q.Id == questionId);
        if (question == null) return false;

        quiz.QuizQuestions.Remove(question);
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizRepository.UpdateAsync(quiz);
        return true;
    }

    public async Task ReorderQuestionsAsync(int quizId, List<ReorderQuestionsRequestDto> request)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        var items = request.SelectMany(r => r.QuestionOrder).ToList();

        foreach (var item in items)
        {
            var question = quiz.QuizQuestions?.FirstOrDefault(q => q.Id == item.Id);
            if (question != null)
                question.OrderPosition = item.OrderPosition;
        }

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.UpdateAsync(quiz);
    }

    // ============================================
    // ✅ OPCIONES - CORREGIDO
    // ============================================

    public async Task<OptionResponseDto> CreateOptionAsync(int questionId, CreateOptionRequestDto request)
    {
        var quiz = await FindQuizByQuestionId(questionId);
        var question = quiz.QuizQuestions!.FirstOrDefault(q => q.Id == questionId);
        if (question == null)
            throw new KeyNotFoundException($"Pregunta con ID {questionId} no encontrada");

        var option = new QuizOption
        {
            QuestionId = questionId,
            OptionText = request.OptionText,
            IsCorrect = request.IsCorrect,
            OrderPosition = request.OrderPosition,
            CreatedAt = DateTime.UtcNow
        };

        if (question.QuizOptions == null)
            question.QuizOptions = new List<QuizOption>();
        question.QuizOptions.Add(option);

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.UpdateAsync(quiz);

        return MapToOptionResponse(option);
    }

    public async Task<OptionResponseDto> UpdateOptionAsync(int optionId, UpdateOptionRequestDto request)
    {
        var (quiz, option) = await FindQuizAndOption(optionId);
        if (option == null)
            throw new KeyNotFoundException($"Opción con ID {optionId} no encontrada");

        if (request.OptionText != null) option.OptionText = request.OptionText;
        if (request.IsCorrect.HasValue) option.IsCorrect = request.IsCorrect.Value;
        if (request.OrderPosition.HasValue) option.OrderPosition = request.OrderPosition.Value;

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.UpdateAsync(quiz);

        return MapToOptionResponse(option);
    }

    public async Task<bool> DeleteOptionAsync(int optionId)
    {
        var (quiz, option) = await FindQuizAndOption(optionId);
        if (option == null) return false;

        var question = quiz.QuizQuestions?.FirstOrDefault(q =>
            q.QuizOptions != null && q.QuizOptions.Any(o => o.Id == optionId));
        question?.QuizOptions?.Remove(option);

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.UpdateAsync(quiz);
        return true;
    }

    // ============================================
    // SETTINGS
    // ============================================

    public async Task UpdateSettingsAsync(int quizId, object settings)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.UpdateAsync(quiz);
    }

    public async Task<object> GetSettingsAsync(int quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        return new
        {
            quiz.PassingScore,
            quiz.TimeLimitMinutes,
            quiz.ShuffleQuestions,
            quiz.ShuffleOptions,
            quiz.AttemptsAllowed
        };
    }

    public async Task ResetSettingsAsync(int quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        quiz.PassingScore = 70.00m;
        quiz.TimeLimitMinutes = null;
        quiz.ShuffleQuestions = false;
        quiz.ShuffleOptions = false;
        quiz.AttemptsAllowed = 0;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizRepository.UpdateAsync(quiz);
    }

    public async Task<object> PreviewAsync(int quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        return new
        {
            quiz.Id,
            quiz.Title,
            quiz.Description,
            quiz.TotalQuestions,
            quiz.PassingScore,
            quiz.TimeLimitMinutes,
            quiz.ShuffleQuestions,
            quiz.ShuffleOptions,
            quiz.AttemptsAllowed,
            Questions = (quiz.QuizQuestions ?? new List<QuizQuestion>())
                .OrderBy(q => q.OrderPosition)
                .Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.QuestionType,
                    q.Points,
                    q.OrderPosition,
                    Options = (q.QuizOptions ?? new List<QuizOption>())
                        .OrderBy(o => o.OrderPosition)
                        .Select(o => new
                        {
                            o.Id,
                            o.OptionText,
                            o.OrderPosition
                        })
                })
        };
    }

    public async Task<object> SimulateAsync(int quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz con ID {quizId} no encontrado");

        var rng = new Random();
        var questions = (quiz.QuizQuestions ?? new List<QuizQuestion>()).ToList();

        if (quiz.ShuffleQuestions)
            questions = questions.OrderBy(_ => rng.Next()).ToList();

        return new
        {
            quiz.Id,
            quiz.Title,
            quiz.TimeLimitMinutes,
            quiz.ShuffleOptions,
            Questions = questions.Select(q => new
            {
                q.Id,
                q.QuestionText,
                q.QuestionType,
                q.Points,
                Options = quiz.ShuffleOptions
                    ? (q.QuizOptions ?? new List<QuizOption>())
                        .OrderBy(_ => rng.Next())
                        .Select(o => new { o.Id, o.OptionText, o.OrderPosition })
                    : (q.QuizOptions ?? new List<QuizOption>())
                        .OrderBy(o => o.OrderPosition)
                        .Select(o => new { o.Id, o.OptionText, o.OrderPosition })
            })
        };
    }

    // ============================================
    // MÉTODOS PRIVADOS
    // ============================================

    private async Task<Quiz> FindQuizByQuestionId(int questionId)
    {
        var quizzes = await _quizRepository.GetAllAsync();
        var quiz = quizzes.FirstOrDefault(q =>
            q.QuizQuestions != null && q.QuizQuestions.Any(qq => qq.Id == questionId));

        if (quiz == null)
            throw new KeyNotFoundException($"Pregunta con ID {questionId} no encontrada");

        return quiz;
    }

    private async Task<(Quiz quiz, QuizOption? option)> FindQuizAndOption(int optionId)
    {
        var quizzes = await _quizRepository.GetAllAsync();
        foreach (var quiz in quizzes)
        {
            var option = quiz.QuizQuestions?
                .SelectMany(q => q.QuizOptions ?? new List<QuizOption>())
                .FirstOrDefault(o => o.Id == optionId);

            if (option != null)
                return (quiz, option);
        }
        return (null!, null);
    }

    private async Task LogActivitySafeAsync(int userId, string action, string entityType, int? entityId, string details)
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
        catch { }
    }

    private QuizResponseDto MapToResponse(Quiz quiz)
    {
        return new QuizResponseDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            CourseId = quiz.CourseId,
            CourseName = quiz.Course?.Name,
            TotalQuestions = quiz.TotalQuestions,
            PassingScore = quiz.PassingScore,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            ShuffleQuestions = quiz.ShuffleQuestions,
            ShuffleOptions = quiz.ShuffleOptions,
            AttemptsAllowed = quiz.AttemptsAllowed,
            UserAttempts = 0,
            BestScore = null,
            AverageScore = null,
            IsCompleted = false,
            LastAttemptAt = null
        };
    }

    private QuizListResponseDto MapToListResponse(Quiz quiz)
    {
        return new QuizListResponseDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            TotalQuestions = quiz.TotalQuestions,
            BestScore = null,
            AverageScore = null,
            AttemptsCount = 0,
            LastAttemptAt = null
        };
    }

    private static QuestionResponseDto MapToQuestionResponse(QuizQuestion question)
    {
        return new QuestionResponseDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText ?? string.Empty,
            QuestionType = question.QuestionType ?? "multiple_choice",
            Explanation = question.Explanation,
            Points = question.Points,
            OrderPosition = question.OrderPosition,
            Options = (question.QuizOptions ?? new List<QuizOption>())
                .OrderBy(o => o.OrderPosition)
                .Select(MapToOptionResponse)
                .ToList()
        };
    }

    private static OptionResponseDto MapToOptionResponse(QuizOption option)
    {
        return new OptionResponseDto
        {
            Id = option.Id,
            OptionText = option.OptionText,
            IsCorrect = option.IsCorrect,
            OrderPosition = option.OrderPosition
        };
    }
}
