using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Progress.Responses.Flashcards;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class FlashcardProgressService : IFlashcardProgressService
{
    private readonly IUserProgressFlashcardRepository _progressRepository;
    private readonly IFlashcardDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public FlashcardProgressService(
        IUserProgressFlashcardRepository progressRepository,
        IFlashcardDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        ICourseRepository courseRepository,
        IActivityLogRepository activityLogRepository)
    {
        _progressRepository = progressRepository;
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _courseRepository = courseRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<FlashcardProgressResponseDto> GetByDeckAsync(int userId, int deckId)
    {
        var progress = await _progressRepository.GetByUserAndDeckAsync(userId, deckId);
        var deck = await _deckRepository.GetByIdAsync(deckId);
        var totalCards = (await _flashcardRepository.GetByDeckIdAsync(deckId)).Count();

        return MapToProgressDto(progress, deck?.Name ?? "Unknown", totalCards);
    }

    public async Task<IEnumerable<FlashcardProgressResponseDto>> GetByCourseAsync(int userId, int courseId)
    {
        var decks = await _deckRepository.GetByCourseIdAsync(courseId);
        var result = new List<FlashcardProgressResponseDto>();

        foreach (var deck in decks)
        {
            var progress = await _progressRepository.GetByUserAndDeckAsync(userId, deck.Id);
            var totalCards = (await _flashcardRepository.GetByDeckIdAsync(deck.Id)).Count();
            result.Add(MapToProgressDto(progress, deck.Name, totalCards));
        }

        return result;
    }

    public async Task<IEnumerable<FlashcardProgressResponseDto>> GetOverallAsync(int userId)
    {
        var allProgress = await _progressRepository.GetByUserAsync(userId);
        var result = new List<FlashcardProgressResponseDto>();

        foreach (var progress in allProgress)
        {
            var deck = await _deckRepository.GetByIdAsync(progress.DeckId);
            var totalCards = (await _flashcardRepository.GetByDeckIdAsync(progress.DeckId)).Count();
            result.Add(MapToProgressDto(progress, deck?.Name ?? "Unknown", totalCards));
        }

        return result;
    }

    public async Task<FlashcardMasteryResponseDto> GetMasteryLevelAsync(int userId, int deckId)
    {
        var progress = await _progressRepository.GetByUserAndDeckAsync(userId, deckId);
        var cards = await _flashcardRepository.GetByDeckIdAsync(deckId);
        var firstCard = cards.FirstOrDefault();

        return new FlashcardMasteryResponseDto
        {
            FlashcardId = firstCard?.Id ?? 0,
            Question = firstCard?.Question ?? string.Empty,
            EaseFactor = progress?.AverageEaseFactor ?? 2.50m,
            RepetitionCount = progress?.TotalReviews ?? 0,
            LastReviewDate = progress?.LastStudiedAt,
            NextReviewDate = progress?.LastStudiedAt?.AddDays(1),
            MasteryLevel = GetMasteryLevel(progress)
        };
    }

    public async Task<MasteryTrendResponseDto> GetMasteryTrendAsync(int userId, int deckId)
    {
        var progress = await _progressRepository.GetByUserAndDeckAsync(userId, deckId);
        var now = DateTime.UtcNow;

        return new MasteryTrendResponseDto
        {
            Dates = Enumerable.Range(0, 7).Select(i => now.AddDays(-6 + i).Date).ToList(),
            MasteryScores = Enumerable.Range(0, 7).Select(i => i == 6 ? progress?.AverageEaseFactor ?? 0 : 0m).ToList(),
            NewCards = Enumerable.Range(0, 7).Select(_ => 0).ToList(),
            LearnedCards = Enumerable.Range(0, 7).Select(_ => 0).ToList(),
            MasteredCards = Enumerable.Range(0, 7).Select(i => i == 6 ? progress?.CardsMastered ?? 0 : 0).ToList(),
            ReviewDueCards = Enumerable.Range(0, 7).Select(i => i == 6 ? progress?.CardsInLearning ?? 0 : 0).ToList()
        };
    }

    public async Task<object> GetPredictionsAsync(int userId, int deckId)
    {
        var progress = await _progressRepository.GetByUserAndDeckAsync(userId, deckId);
        var deck = await _deckRepository.GetByIdAsync(deckId);
        var totalCards = (await _flashcardRepository.GetByDeckIdAsync(deckId)).Count();

        return new
        {
            EstimatedMasteryDate = DateTime.UtcNow.AddDays(14),
            ProjectedMasteryPercentage = progress?.CardsMastered > 0
                ? Math.Min(100, (decimal)progress.CardsMastered * 100 / Math.Max(1, totalCards))
                : 0,
            CardsToReviewPerDay = Math.Max(1, (totalCards - (progress?.CardsMastered ?? 0)) / 14),
            ConfidenceLevel = progress?.AverageEaseFactor ?? 2.50m
        };
    }

    public async Task<IEnumerable<WeeklyFlashcardProgressResponseDto>> GetTimelineAsync(int userId, int deckId)
    {
        var now = DateTime.UtcNow;
        var weeks = new List<WeeklyFlashcardProgressResponseDto>();

        for (int i = 4; i >= 0; i--)
        {
            var weekStart = now.AddDays(-i * 7).Date;
            weeks.Add(new WeeklyFlashcardProgressResponseDto
            {
                WeekStart = weekStart,
                WeekEnd = weekStart.AddDays(7),
                CardsReviewed = 0,
                NewCardsLearned = i == 0 ? 5 : 0,
                CardsMastered = i == 0 ? 3 : 0,
                AverageEaseFactor = 2.50m
            });
        }

        return weeks;
    }

    public async Task<IEnumerable<WeeklyFlashcardProgressResponseDto>> GetWeeklyProgressAsync(int userId)
    {
        return await GetTimelineAsync(userId, 0);
    }

    public async Task<object> GetMonthlyReportAsync(int userId, int month, int year)
    {
        var allProgress = await _progressRepository.GetByUserAsync(userId);

        return new
        {
            Month = month,
            Year = year,
            TotalCardsReviewed = allProgress.Sum(p => p.TotalReviews),
            TotalCardsMastered = allProgress.Sum(p => p.CardsMastered),
            TotalStudySessions = allProgress.Sum(p => p.TotalStudySessions),
            AverageEaseFactor = allProgress.Any() ? allProgress.Average(p => p.AverageEaseFactor) : 0,
            DecksProgress = allProgress.Select(p => new
            {
                DeckId = p.DeckId,
                CardsMastered = p.CardsMastered,
                CardsInLearning = p.CardsInLearning
            })
        };
    }

    // ============================================
    // ✅ MAPEO CORREGIDO
    // ============================================

    private static FlashcardProgressResponseDto MapToProgressDto(UserProgressFlashcard? progress, string deckName, int totalCards)
    {
        int mastered = progress?.CardsMastered ?? 0;
        int learning = progress?.CardsInLearning ?? 0;
        int studied = mastered + learning;
        int notStarted = Math.Max(0, totalCards - studied);

        return new FlashcardProgressResponseDto
        {
            DeckId = progress?.DeckId ?? 0,
            DeckName = deckName,
            TotalCards = totalCards,
            StudiedCount = studied,
            MasteredCount = mastered,
            LearningCount = learning,
            NotStartedCount = notStarted,
            MasteryPercentage = totalCards > 0
                ? Math.Min(100, mastered * 100m / totalCards)
                : 0,
            LastStudiedAt = progress?.LastStudiedAt
        };
    }

    private static string GetMasteryLevel(UserProgressFlashcard? progress)
    {
        if (progress == null) return "NotStarted";
        var ratio = progress.CardsMastered > 0
            ? (decimal)progress.CardsMastered / Math.Max(1, progress.CardsMastered + progress.CardsInLearning)
            : 0;
        return ratio >= 0.8m ? "Mastered" : ratio >= 0.5m ? "Learning" : "Beginner";
    }
}
