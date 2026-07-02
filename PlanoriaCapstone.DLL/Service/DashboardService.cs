using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Dashboard.Responses;
using PlanoriaCapstone.DTOs.Dashboard.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IStudyScheduleRepository _scheduleRepository;
    private readonly IUserProgressFlashcardRepository _flashcardProgressRepository;
    private readonly IUserProgressQuizRepository _quizProgressRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly INotificationRepository _notificationRepository;

    public DashboardService(
        AppDbContext context,
        IActivityLogRepository activityLogRepository,
        IStudyScheduleRepository scheduleRepository,
        IUserProgressFlashcardRepository flashcardProgressRepository,
        IUserProgressQuizRepository quizProgressRepository,
        ICourseRepository courseRepository,
        INotificationRepository notificationRepository)
    {
        _context = context;
        _activityLogRepository = activityLogRepository;
        _scheduleRepository = scheduleRepository;
        _flashcardProgressRepository = flashcardProgressRepository;
        _quizProgressRepository = quizProgressRepository;
        _courseRepository = courseRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<DashboardOverviewResponseDto> GetSummaryAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.AddDays(-(int)now.DayOfWeek).Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var todayLogs = await _activityLogRepository.GetByUserAsync(userId, 100);
        var todayActivities = todayLogs.Where(l => l.CreatedAt >= todayStart).ToList();
        var weekActivities = todayLogs.Where(l => l.CreatedAt >= weekStart).ToList();
        var monthActivities = todayLogs.Where(l => l.CreatedAt >= monthStart).ToList();

        var flashcardProgress = await _flashcardProgressRepository.GetByUserAsync(userId);
        var quizProgress = await _quizProgressRepository.GetByUserAsync(userId);
        var courses = await _courseRepository.GetByUserIdAsync(userId);
        var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

        var todaySessions = await _context.FlashcardStudySessions
            .Where(s => s.UserId == userId && s.EndedAt != null && s.EndedAt >= todayStart)
            .ToListAsync();
        var weekSessions = await _context.FlashcardStudySessions
            .Where(s => s.UserId == userId && s.EndedAt != null && s.EndedAt >= weekStart)
            .ToListAsync();
        var monthSessions = await _context.FlashcardStudySessions
            .Where(s => s.UserId == userId && s.EndedAt != null && s.EndedAt >= monthStart)
            .ToListAsync();

        var todayMinutes = (int)todaySessions.Sum(s => (s.EndedAt!.Value - s.StartedAt).TotalMinutes);
        var weekMinutes = (int)weekSessions.Sum(s => (s.EndedAt!.Value - s.StartedAt).TotalMinutes);
        var monthMinutes = (int)monthSessions.Sum(s => (s.EndedAt!.Value - s.StartedAt).TotalMinutes);

        var dueFlashcards = flashcardProgress.Sum(p => p.CardsInLearning);
        var pendingQuizzes = quizProgress.Count(qp => qp.TotalAttempts == 0);

        return new DashboardOverviewResponseDto
        {
            TotalStudyTimeToday = todayMinutes,
            TotalStudyTimeWeek = weekMinutes,
            TotalStudyTimeMonth = monthMinutes,
            CardsReviewedToday = flashcardProgress.Sum(p => p.TotalReviews),
            QuizzesCompletedToday = quizProgress.Sum(p => p.TotalAttempts),
            StreakDays = CalculateStreak(todayLogs),
            UpcomingExamsCount = courses.Count(c => c.ExamDate.HasValue && c.ExamDate > now),
            PendingReviewsCount = dueFlashcards + pendingQuizzes
        };
    }

    public async Task<IEnumerable<ActivityItemResponseDto>> GetRecentActivityAsync(int userId, int limit)
    {
        var logs = await _activityLogRepository.GetByUserAsync(userId, limit);
        return logs.Select(l => new ActivityItemResponseDto
        {
            Type = l.Action,
            Title = l.Details ?? l.Action,
            CourseName = string.Empty,
            Timestamp = l.CreatedAt,
            Action = l.Action,
            Metadata = l.EntityType
        });
    }

    public async Task<IEnumerable<UpcomingDeadlineResponseDto>> GetUpcomingDeadlinesAsync(int userId, int days)
    {
        var courses = await _courseRepository.GetByUserIdAsync(userId);
        var now = DateTime.UtcNow;
        var limit = now.AddDays(days);

        var deadlines = courses
            .Where(c => c.ExamDate.HasValue && c.ExamDate >= now && c.ExamDate <= limit)
            .Select(c =>
            {
                var daysRemaining = (int)(c.ExamDate!.Value - now).TotalDays;
                return new UpcomingDeadlineResponseDto
                {
                    Type = "Exam",
                    Title = $"{c.Name} Exam",
                    CourseName = c.Name,
                    DueDate = c.ExamDate.Value,
                    DaysRemaining = daysRemaining,
                    Urgency = daysRemaining <= 7 ? "High" : daysRemaining <= 30 ? "Medium" : "Low"
                };
            });

        return deadlines;
    }

    public async Task<MetricCardResponseDto> GetStudyTimeAsync(int userId, string period)
    {
        var now = DateTime.UtcNow;

        var query = _context.FlashcardStudySessions
            .Where(s => s.UserId == userId && s.EndedAt != null);

        var startDate = period?.ToLower() switch
        {
            "today" => now.Date,
            "week" => now.AddDays(-7),
            "month" => now.AddMonths(-1),
            _ => DateTime.MinValue
        };

        if (startDate > DateTime.MinValue)
            query = query.Where(s => s.EndedAt >= startDate);

        var sessions = await query.ToListAsync();
        var totalMinutes = (int)sessions.Sum(s => (s.EndedAt!.Value - s.StartedAt).TotalMinutes);

        return new MetricCardResponseDto
        {
            Title = $"Study Time ({period ?? "all"})",
            Value = totalMinutes,
            Change = 0,
            ChangeType = "neutral",
            Icon = "clock",
            Color = "blue"
        };
    }

    public async Task<MetricCardResponseDto> GetCardsReviewedAsync(int userId, string period)
    {
        var progress = await _flashcardProgressRepository.GetByUserAsync(userId);
        var now = DateTime.UtcNow;

        var total = period?.ToLower() switch
        {
            "today" => progress.Where(p => p.LastStudiedAt >= now.Date).Sum(p => p.TotalReviews),
            "week" => progress.Where(p => p.LastStudiedAt >= now.AddDays(-7)).Sum(p => p.TotalReviews),
            "month" => progress.Where(p => p.LastStudiedAt >= now.AddMonths(-1)).Sum(p => p.TotalReviews),
            _ => progress.Sum(p => p.TotalReviews)
        };

        return new MetricCardResponseDto
        {
            Title = $"Cards Reviewed ({period ?? "all"})",
            Value = total,
            Change = 0,
            ChangeType = "neutral",
            Icon = "cards",
            Color = "green"
        };
    }

    public async Task<MetricCardResponseDto> GetQuizzesCompletedAsync(int userId, string period)
    {
        var progress = await _quizProgressRepository.GetByUserAsync(userId);
        var total = progress.Sum(p => p.TotalAttempts);

        return new MetricCardResponseDto
        {
            Title = $"Quizzes Completed ({period ?? "all"})",
            Value = total,
            Change = 0,
            ChangeType = "neutral",
            Icon = "quiz",
            Color = "purple"
        };
    }

    public async Task<ChartDataResponseDto> GetProgressChartAsync(int userId, int? courseId, string period)
    {
        var now = DateTime.UtcNow;
        var labels = new List<string>();
        var dataPoints = new List<decimal>();

        for (int i = 6; i >= 0; i--)
        {
            var day = now.AddDays(-i);
            labels.Add(day.ToString("ddd"));
            dataPoints.Add(0);
        }

        return new ChartDataResponseDto
        {
            Labels = labels,
            Datasets = new List<DatasetDto>
            {
                new() { Label = "Progress", Data = dataPoints, BackgroundColor = "#3498db", BorderColor = "#2980b9", Fill = false }
            }
        };
    }

    public async Task<HeatmapDataResponseDto> GetHeatmapDataAsync(int userId, int? year)
    {
        var yearToUse = year ?? DateTime.UtcNow.Year;
        var logs = await _activityLogRepository.GetByUserAsync(userId, 500);
        var startOfYear = new DateTime(yearToUse, 1, 1);

        var days = logs
            .Where(l => l.CreatedAt.Year == yearToUse)
            .GroupBy(l => l.CreatedAt.Date)
            .Select(g => new HeatmapDay
            {
                Date = g.Key,
                Intensity = Math.Min(4, g.Count())
            })
            .ToList();

        return new HeatmapDataResponseDto
        {
            Year = yearToUse,
            Days = days,
            TotalActivity = days.Sum(d => d.Intensity)
        };
    }

    public async Task<DistributionDataResponseDto> GetDistributionDataAsync(int userId, int? courseId)
    {
        var progress = await _flashcardProgressRepository.GetByUserAsync(userId);

        return new DistributionDataResponseDto
        {
            Labels = new List<string> { "Mastered", "Learning", "Not Started" },
            Values = new List<decimal>
            {
                progress.Sum(p => p.CardsMastered),
                progress.Sum(p => p.CardsInLearning),
                0
            },
            Total = progress.Sum(p => p.CardsMastered + p.CardsInLearning)
        };
    }

    public async Task<byte[]> ExportToPdfAsync(int userId, ExportDashboardRequestDto request)
    {
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<string> ExportToCsvAsync(int userId, ExportDashboardRequestDto request)
    {
        var summary = await GetSummaryAsync(userId);
        return $"TotalStudyTimeToday,{summary.TotalStudyTimeToday}\nTotalStudyTimeWeek,{summary.TotalStudyTimeWeek}\nCardsReviewedToday,{summary.CardsReviewedToday}\nQuizzesCompletedToday,{summary.QuizzesCompletedToday}\nStreakDays,{summary.StreakDays}";
    }

    public async Task<object> GenerateReportAsync(int userId, ExportDashboardRequestDto request)
    {
        return new
        {
            GeneratedAt = DateTime.UtcNow,
            Format = request.Format,
            IncludesCharts = request.IncludeCharts,
            IncludesRawData = request.IncludeRawData,
            Data = await GetSummaryAsync(userId)
        };
    }

    private static int CalculateStreak(IEnumerable<ActivityLog> logs)
    {
        var days = logs
            .Select(l => l.CreatedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (!days.Any()) return 0;

        var streak = 0;
        var expected = DateTime.UtcNow.Date;

        foreach (var day in days)
        {
            if (day == expected)
            {
                streak++;
                expected = expected.AddDays(-1);
            }
            else if (day < expected)
            {
                break;
            }
        }

        return streak;
    }
}
