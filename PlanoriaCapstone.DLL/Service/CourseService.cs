using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Courses.Requests;
using PlanoriaCapstone.DTOs.Courses.Responses;
using PlanoriaCapstone.DTOs.Users.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUserCourseExamProgressRepository _progressRepository;
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IStudyScheduleRepository _scheduleRepository;
    private readonly IFlashcardDeckRepository _deckRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IUserProgressFlashcardRepository _flashcardProgressRepo;
    private readonly IUserProgressQuizRepository _quizProgressRepo;

    public CourseService(
        ICourseRepository courseRepository,
        IUserCourseExamProgressRepository progressRepository,
        IUserRepository userRepository,
        IActivityLogRepository activityLogRepository,
        IStudyScheduleRepository scheduleRepository,
        IFlashcardDeckRepository deckRepository,
        IQuizRepository quizRepository,
        IUserProgressFlashcardRepository flashcardProgressRepo,
        IUserProgressQuizRepository quizProgressRepo)
    {
        _courseRepository = courseRepository;
        _progressRepository = progressRepository;
        _userRepository = userRepository;
        _activityLogRepository = activityLogRepository;
        _scheduleRepository = scheduleRepository;
        _deckRepository = deckRepository;
        _quizRepository = quizRepository;
        _flashcardProgressRepo = flashcardProgressRepo;
        _quizProgressRepo = quizProgressRepo;
    }

    //  GESTION DE CURSOS


    // Obtiene la lista de cursos del usuario con su progreso y los ordena por fecha de examen.
    public async Task<IEnumerable<CourseListResponseDto>> GetByUserIdAsync(int userId)
    {
        var courses = await _courseRepository.GetByUserIdAsync(userId);
        var allFlashcardProgress = (await _flashcardProgressRepo.GetByUserAsync(userId)).ToList();
        var allQuizProgress = (await _quizProgressRepo.GetByUserAsync(userId)).ToList();
        var dtos = new List<CourseListResponseDto>();

        foreach (var course in courses)
        {
            var progress = await _progressRepository.GetByUserAndCourseAsync(userId, course.Id);
            var realProgress = await CalculateRealProgressAsync(userId, course.Id, allFlashcardProgress, allQuizProgress);
            dtos.Add(MapToListDto(course, progress, realProgress));
        }

        return dtos.OrderByDescending(c => c.ExamDate ?? DateTime.MaxValue);
    }

    private async Task<decimal> CalculateRealProgressAsync(int userId, int courseId,
        List<UserProgressFlashcard> allFlashcardProgress, List<UserProgressQuiz> allQuizProgress)
    {
        var decks = (await _deckRepository.GetByCourseIdAsync(courseId)).ToList();
        var deckIds = decks.Select(d => d.Id).ToHashSet();
        var totalCards = decks.Sum(d => d.TotalCards);
        var masteredCards = allFlashcardProgress.Where(p => deckIds.Contains(p.DeckId)).Sum(p => p.CardsMastered);

        var quizzes = (await _quizRepository.GetByCourseIdAsync(courseId)).ToList();
        var quizIds = quizzes.Select(q => q.Id).ToHashSet();
        var totalQuizzes = quizzes.Count;
        var completedQuizzes = allQuizProgress.Where(p => quizIds.Contains(p.QuizId)).Sum(p => p.TotalAttempts);

        var totalItems = totalCards + totalQuizzes;
        if (totalItems == 0) return 0;

        return Math.Round((decimal)(masteredCards + completedQuizzes) / totalItems * 100, 1);
    }

    // Busca el curso y cuánto ha avanzado el usuario en ese curso.
    public async Task<CourseResponseDto?> GetByIdAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return null;

        var progress = await _progressRepository.GetByUserAndCourseAsync(course.UserId, id);

        var totalFlashcards = (await _deckRepository.GetByCourseIdAsync(id))
            .Sum(d => d.TotalCards);
        var totalQuizzes = (await _quizRepository.GetByCourseIdAsync(id, course.UserId))
            .Count();

        var allFlashcardProgress = (await _flashcardProgressRepo.GetByUserAsync(course.UserId)).ToList();
        var allQuizProgress = (await _quizProgressRepo.GetByUserAsync(course.UserId)).ToList();
        var realProgress = await CalculateRealProgressAsync(course.UserId, id, allFlashcardProgress, allQuizProgress);

        return MapToResponseDto(course, progress, totalFlashcards, totalQuizzes, realProgress);
    }

    public async Task<CourseResponseDto> CreateAsync(int userId, CreateCourseRequestDto request)
    {
        var course = new Course
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            ColorHex = request.ColorHex ?? "#3498db",
            ExamDate = request.ExamDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(request.ExamTime) && TimeSpan.TryParse(request.ExamTime, out var examTime))
        {
            course.ExamTime = examTime;
        }

        course.UserCourses = new List<UserCourse>
        {
            new UserCourse
            {
                UserId = userId,
                Role = "owner",
                JoinedAt = DateTime.UtcNow
            }
        };

        var created = await _courseRepository.CreateAsync(course);

        if (created.ExamDate.HasValue)
        {
            var schedule = new StudySchedule
            {
                UserId = userId,
                Title = $"Examen: {created.Name}",
                StartDatetime = created.ExamDate.Value,
                EndDatetime = created.ExamDate.Value.AddHours(2),
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _scheduleRepository.CreateAsync(schedule);
        }

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "course.created",
            EntityType = "Course",
            EntityId = created.Id,
            Details = $"Created course '{created.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        return MapToResponseDto(created, null);
    }
    public async Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseRequestDto request)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return null;

        course.Name = request.Name ?? course.Name;
        course.Description = request.Description ?? course.Description;
        course.ColorHex = request.ColorHex ?? course.ColorHex;

        if (request.IsArchived.HasValue)
            course.IsArchived = request.IsArchived.Value;

        course.ExamDate = request.ExamDate;

        if (request.ExamTime != null)
        {
            course.ExamTime = TimeSpan.TryParse(request.ExamTime, out var examTime) ? examTime : null;
        }
        else
        {
            course.ExamTime = null;
        }

        course.UpdatedAt = DateTime.UtcNow;

        var updated = await _courseRepository.UpdateAsync(course);

        if (request.ExamDate.HasValue)
        {
            var existingSchedules = await _scheduleRepository.GetByUserAsync(course.UserId);
            var examSchedule = existingSchedules.FirstOrDefault(s =>
                s.Title == $"Examen: {updated.Name}");

            if (examSchedule != null)
            {
                examSchedule.StartDatetime = request.ExamDate.Value;
                examSchedule.EndDatetime = request.ExamDate.Value.AddHours(2);
                await _scheduleRepository.UpdateAsync(examSchedule);
            }
            else
            {
                var schedule = new StudySchedule
                {
                    UserId = course.UserId,
                    Title = $"Examen: {updated.Name}",
                    StartDatetime = request.ExamDate.Value,
                    EndDatetime = request.ExamDate.Value.AddHours(2),
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _scheduleRepository.CreateAsync(schedule);
            }
        }

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = course.UserId,
            Action = "course.updated",
            EntityType = "Course",
            EntityId = updated.Id,
            CreatedAt = DateTime.UtcNow
        });

        var progress = await _progressRepository.GetByUserAndCourseAsync(course.UserId, id);
        return MapToResponseDto(updated, progress);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        var result = await _courseRepository.DeleteAsync(id);

        if (result)
        {
            await _activityLogRepository.LogAsync(new ActivityLog
            {
                UserId = course.UserId,
                Action = "course.deleted",
                EntityType = "Course",
                EntityId = id,
                CreatedAt = DateTime.UtcNow
            });
        }

        return result;
    }

    public async Task ArchiveAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return;

        course.IsArchived = true;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = course.UserId,
            Action = "course.archived",
            EntityType = "Course",
            EntityId = id,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task RestoreAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return;

        course.IsArchived = false;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = course.UserId,
            Action = "course.restored",
            EntityType = "Course",
            EntityId = id,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<IEnumerable<CourseListResponseDto>> SearchAsync(int userId, CourseSearchRequestDto request)
    {
        var courses = await _courseRepository.GetByUserIdAsync(userId);

        if (!string.IsNullOrEmpty(request.Query))
        {
            var query = request.Query.ToLower();
            courses = courses.Where(c =>
                c.Name.ToLower().Contains(query) ||
                (c.Description?.ToLower().Contains(query) ?? false));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (request.Status.ToLower() == "archived")
                courses = courses.Where(c => c.IsArchived);
            else if (request.Status.ToLower() == "active")
                courses = courses.Where(c => !c.IsArchived);
        }

        courses = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc"
                ? courses.OrderByDescending(c => c.Name)
                : courses.OrderBy(c => c.Name),
            "examdate" => request.SortOrder?.ToLower() == "desc"
                ? courses.OrderByDescending(c => c.ExamDate)
                : courses.OrderBy(c => c.ExamDate),
            "created" => request.SortOrder?.ToLower() == "desc"
                ? courses.OrderByDescending(c => c.CreatedAt)
                : courses.OrderBy(c => c.CreatedAt),
            _ => courses.OrderByDescending(c => c.CreatedAt)
        };

        var allFlashcardProgress = (await _flashcardProgressRepo.GetByUserAsync(userId)).ToList();
        var allQuizProgress = (await _quizProgressRepo.GetByUserAsync(userId)).ToList();
        var dtos = new List<CourseListResponseDto>();
        foreach (var course in courses)
        {
            var progress = await _progressRepository.GetByUserAndCourseAsync(userId, course.Id);
            var realProgress = await CalculateRealProgressAsync(userId, course.Id, allFlashcardProgress, allQuizProgress);
            dtos.Add(MapToListDto(course, progress, realProgress));
        }

        return dtos;
    }

    public async Task<CourseStatsResponseDto> GetStatsAsync(int courseId, int userId)
    {
        var progress = await _progressRepository.GetByUserAndCourseAsync(userId, courseId);

        return new CourseStatsResponseDto
        {
            TotalFlashcards = progress?.TotalFlashcards ?? 0,
            FlashcardsMastered = progress?.FlashcardsMastered ?? 0,
            TotalQuizzes = progress?.TotalQuizzes ?? 0,
            QuizzesPassed = progress?.QuizzesPassed ?? 0,
            AverageQuizScore = progress?.ExamReadinessScore ?? 0,
            StudyTimeHours = 0,
            LastActiveAt = progress?.LastCalculatedAt
        };
    }

    //GESTION DE FECHAS DE EXAMENES

    public async Task SetExamDateAsync(int id, SetExamDateRequestDto request)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return;

        course.ExamDate = request.ExamDate;

        if (!string.IsNullOrEmpty(request.ExamTime) && TimeSpan.TryParse(request.ExamTime, out var examTime))
            course.ExamTime = examTime;

        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        var existingSchedules = await _scheduleRepository.GetByUserAsync(course.UserId);
        var examSchedule = existingSchedules.FirstOrDefault(s =>
            s.Title == $"Examen: {course.Name}");

        if (examSchedule != null)
        {
            examSchedule.StartDatetime = request.ExamDate;
            examSchedule.EndDatetime = request.ExamDate.AddHours(2);
            await _scheduleRepository.UpdateAsync(examSchedule);
        }
        else
        {
            var schedule = new StudySchedule
            {
                UserId = course.UserId,
                Title = $"Examen: {course.Name}",
                StartDatetime = request.ExamDate,
                EndDatetime = request.ExamDate.AddHours(2),
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _scheduleRepository.CreateAsync(schedule);
        }

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = course.UserId,
            Action = "course.exam_date_set",
            EntityType = "Course",
            EntityId = id,
            Details = $"Exam date set to {request.ExamDate:yyyy-MM-dd}",
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<CourseExamResponseDto?> GetExamDateAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return null;

        var progress = await _progressRepository.GetByUserAndCourseAsync(course.UserId, id);

        var dto = new CourseExamResponseDto
        {
            ExamDate = course.ExamDate,
            ExamTime = course.ExamTime?.ToString(@"hh\:mm") ?? string.Empty
        };

        if (course.ExamDate.HasValue)
        {
            var daysRemaining = (int)(course.ExamDate.Value.Date - DateTime.UtcNow.Date).TotalDays;
            dto.DaysRemaining = daysRemaining >= 0 ? daysRemaining : 0;
            dto.IsOverdue = daysRemaining < 0;
        }

        dto.ReadinessScore = progress?.ExamReadinessScore ?? 0;

        return dto;
    }

    public async Task RemoveExamDateAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return;

        course.ExamDate = null;
        course.ExamTime = null;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = course.UserId,
            Action = "course.exam_date_removed",
            EntityType = "Course",
            EntityId = id,
            CreatedAt = DateTime.UtcNow
        });
    }


    //CURSOS COMPARTIDOS
    public async Task<IEnumerable<CourseMemberResponseDto>> GetMembersAsync(int courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course?.UserCourses == null)
            return Enumerable.Empty<CourseMemberResponseDto>();

        var members = new List<CourseMemberResponseDto>();

        foreach (var uc in course.UserCourses)
        {
            if (uc.User == null) continue;

            members.Add(new CourseMemberResponseDto
            {
                Id = uc.Id,
                User = new UserResponseDto
                {
                    Id = uc.User.Id,
                    FullName = uc.User.FullName,
                    Email = uc.User.Email
                },
                Role = uc.Role,
                JoinedAt = uc.JoinedAt
            });
        }

        return members;
    }

    public async Task AddMemberAsync(int courseId, int userId, AddCourseMemberRequestDto request)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null) return;

        course.UserCourses ??= new List<UserCourse>();

        if (course.UserCourses.Any(uc => uc.UserId == request.UserId))
            return;

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) return;

        course.UserCourses.Add(new UserCourse
        {
            UserId = request.UserId,
            CourseId = courseId,
            Role = request.Role,
            JoinedAt = DateTime.UtcNow
        });

        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "course.member_added",
            EntityType = "Course",
            EntityId = courseId,
            Details = $"Added user {request.UserId} as {request.Role}",
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task RemoveMemberAsync(int courseId, int userId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course?.UserCourses == null) return;

        var userCourse = course.UserCourses.FirstOrDefault(uc => uc.UserId == userId);
        if (userCourse == null) return;

        course.UserCourses.Remove(userCourse);
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "course.member_removed",
            EntityType = "Course",
            EntityId = courseId,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task ChangeMemberRoleAsync(int courseId, int targetUserId, UpdateMemberRoleRequestDto request)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course?.UserCourses == null) return;

        var userCourse = course.UserCourses.FirstOrDefault(uc => uc.UserId == targetUserId);
        if (userCourse == null) return;

        userCourse.Role = request.Role;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = course.UserId,
            Action = "course.member_role_changed",
            EntityType = "Course",
            EntityId = courseId,
            Details = $"User {targetUserId} role changed to {request.Role}",
            CreatedAt = DateTime.UtcNow
        });
    }


    //UTILIDADES
    private CourseResponseDto MapToResponseDto(Course course, UserCourseExamProgress? progress, int totalFlashcards = 0, int totalQuizzes = 0, decimal realProgress = 0)
    {
        return new CourseResponseDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description ?? string.Empty,
            ExamDate = course.ExamDate,
            ExamTime = course.ExamTime?.ToString(@"hh\:mm") ?? string.Empty,
            ColorHex = course.ColorHex,
            IsArchived = course.IsArchived,
            TotalFlashcards = totalFlashcards,
            TotalQuizzes = totalQuizzes,
            ProgressPercentage = realProgress > 0 ? realProgress : (progress?.ExamReadinessScore ?? 0),
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }

    private CourseListResponseDto MapToListDto(Course course, UserCourseExamProgress? progress, decimal realProgress)
    {
        return new CourseListResponseDto
        {
            Id = course.Id,
            Name = course.Name,
            ColorHex = course.ColorHex,
            ExamDate = course.ExamDate,
            ProgressPercentage = realProgress > 0 ? realProgress : (progress?.ExamReadinessScore ?? 0),
            IsArchived = course.IsArchived,
            LastStudiedAt = progress?.LastCalculatedAt
        };
    }

}
