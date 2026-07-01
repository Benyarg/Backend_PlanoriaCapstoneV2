using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Progress.Responses.Exam;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class CourseProgressService : ICourseProgressService
{
    private readonly IUserCourseExamProgressRepository _examProgressRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFlashcardDeckRepository _deckRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public CourseProgressService(
        IUserCourseExamProgressRepository examProgressRepository,
        ICourseRepository courseRepository,
        IFlashcardDeckRepository deckRepository,
        IQuizRepository quizRepository,
        IActivityLogRepository activityLogRepository)
    {
        _examProgressRepository = examProgressRepository;
        _courseRepository = courseRepository;
        _deckRepository = deckRepository;
        _quizRepository = quizRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<CourseExamProgressResponseDto> GetExamProgressAsync(int userId, int courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        var progress = await _examProgressRepository.GetByUserAndCourseAsync(userId, courseId);

        var daysRemaining = course?.ExamDate.HasValue == true
            ? (int)(course.ExamDate.Value - DateTime.UtcNow).TotalDays
            : 0;

        var totalFlashcards = (await _deckRepository.GetByCourseIdAsync(courseId)).Sum(d => d.TotalCards);
        var totalQuizzes = (await _quizRepository.GetByCourseIdAsync(courseId)).Count();

        return new CourseExamProgressResponseDto
        {
            CourseId = courseId,
            CourseName = course?.Name ?? "Unknown",
            ExamDate = course?.ExamDate,
            DaysRemaining = Math.Max(0, daysRemaining),
            ExamReadinessScore = progress?.ExamReadinessScore ?? 0,
            TotalProgressPercentage = progress?.TotalFlashcards > 0
                ? (decimal)(progress.FlashcardsStudied + progress.QuizzesCompleted) / Math.Max(1, progress.TotalFlashcards + progress.TotalQuizzes) * 100
                : 0,
            RequiredDailyCards = totalFlashcards / Math.Max(1, daysRemaining),
            RequiredDailyQuizzes = totalQuizzes / Math.Max(1, daysRemaining),
            IsOnTrack = (progress?.ExamReadinessScore ?? 0) >= 70
        };
    }

    public async Task<ReadinessScoreResponseDto> GetReadinessScoreAsync(int userId, int courseId)
    {
        var progress = await _examProgressRepository.GetByUserAndCourseAsync(userId, courseId);
        var history = await _examProgressRepository.GetReadinessHistoryAsync(userId, courseId);
        var previous = history.OrderByDescending(h => h.CalculatedAt).Skip(1).FirstOrDefault();

        var currentScore = progress?.ExamReadinessScore ?? 0;
        var previousScore = previous?.Score ?? 0;

        return new ReadinessScoreResponseDto
        {
            CurrentScore = currentScore,
            PreviousScore = previousScore,
            ChangePercentage = previousScore > 0 ? ((currentScore - previousScore) / previousScore) * 100 : 0,
            Factors = new ReadinessFactors
            {
                FlashcardsMastery = progress?.TotalFlashcards > 0
                    ? (decimal)progress.FlashcardsMastered / progress.TotalFlashcards * 100 : 0,
                QuizzesPerformance = progress?.TotalQuizzes > 0
                    ? (decimal)progress.QuizzesPassed / progress.TotalQuizzes * 100 : 0,
                StudyConsistency = 50,
                TimeUntilExam = 50
            }
        };
    }

    public async Task<IEnumerable<object>> GetRecommendationsAsync(int userId, int courseId)
    {
        var progress = await _examProgressRepository.GetByUserAndCourseAsync(userId, courseId);
        var recommendations = new List<object>();

        if (progress == null || progress.ExamReadinessScore < 50)
            recommendations.Add(new { Action = "Increase study time", Priority = "High", Impact = "Critical" });

        if (progress?.FlashcardsMastered < progress?.TotalFlashcards * 0.5)
            recommendations.Add(new { Action = "Focus on flashcard mastery", Priority = "Medium", Impact = "Significant" });

        if (progress?.QuizzesPassed < progress?.TotalQuizzes * 0.6)
            recommendations.Add(new { Action = "Complete more quizzes", Priority = "Medium", Impact = "Moderate" });

        return recommendations;
    }

    public async Task<IEnumerable<ReadinessHistoryResponseDto>> GetReadinessHistoryAsync(int userId, int courseId)
    {
        var history = await _examProgressRepository.GetReadinessHistoryAsync(userId, courseId);

        return new List<ReadinessHistoryResponseDto>
        {
            new ReadinessHistoryResponseDto
            {
                History = history.OrderBy(h => h.CalculatedAt).Select(h => new ReadinessPoint
                {
                    Date = h.CalculatedAt,
                    Score = h.Score
                }).ToList(),
                Trend = DetermineTrend(history),
                PredictedScoreOnExamDate = history.Any() ? history.Average(h => h.Score) : 0,
                ConfidenceInterval = 10
            }
        };
    }

    public async Task<IEnumerable<ReadinessHistoryResponseDto>> GetReadinessTrendAsync(int userId, int courseId)
    {
        return await GetReadinessHistoryAsync(userId, courseId);
    }

    public async Task<object> GetPredictionsAsync(int userId, int courseId)
    {
        var history = await _examProgressRepository.GetReadinessHistoryAsync(userId, courseId);
        var course = await _courseRepository.GetByIdAsync(courseId);

        var avgScore = history.Any() ? history.Average(h => h.Score) : 0;
        var daysUntilExam = course?.ExamDate.HasValue == true
            ? (int)(course.ExamDate.Value - DateTime.UtcNow).TotalDays : 30;

        return new
        {
            PredictedScore = Math.Min(100, avgScore + (100 - avgScore) / Math.Max(1, daysUntilExam) * 7),
            EstimatedDaysToTarget = daysUntilExam,
            ConfidenceLevel = history.Count() >= 5 ? "High" : "Medium",
            RequiredDailyStudyHours = Math.Max(1, (100 - avgScore) / 10)
        };
    }

    public async Task<IEnumerable<WeaknessesResponseDto>> IdentifyWeaknessesAsync(int userId, int courseId)
    {
        var decks = await _deckRepository.GetByCourseIdAsync(courseId);
        var quizzes = await _quizRepository.GetByCourseIdAsync(courseId);
        var weaknesses = new List<WeaknessesResponseDto>();

        foreach (var deck in decks)
        {
            weaknesses.Add(new WeaknessesResponseDto
            {
                WeakTopics = new List<string> { deck.Name },
                RecommendedActions = new List<string> { "Review this deck more frequently" },
                PriorityContent = new List<PriorityContent>
                {
                    new PriorityContent { Type = "Deck", Id = deck.Id, Name = deck.Name }
                },
                EstimatedTimeToImprove = 60
            });
        }

        return weaknesses;
    }

    public async Task<IEnumerable<WeaknessesResponseDto>> GetPriorityTopicsAsync(int userId, int courseId)
    {
        return await IdentifyWeaknessesAsync(userId, courseId);
    }

    public async Task<IEnumerable<WeaknessesResponseDto>> SuggestFocusAsync(int userId, int courseId)
    {
        return await IdentifyWeaknessesAsync(userId, courseId);
    }

    private static string DetermineTrend(IEnumerable<ExamReadinessScore> history)
    {
        var list = history.OrderBy(h => h.CalculatedAt).ToList();
        if (list.Count < 2) return "Stable";

        return list.Last().Score > list.First().Score ? "Improving" : "Declining";
    }
}
