using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Responses;
using PlanoriaCapstone.DTOs.Flashcards.Study.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Study.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class FlashcardStudyService : IFlashcardStudyService
{
    private readonly AppDbContext _context;
    private readonly IFlashcardRepository _flashcardRepo;
    private readonly IFlashcardDeckRepository _deckRepo;
    private readonly IUserProgressFlashcardRepository _progressRepo;
    private readonly IActivityLogRepository _logRepo;

    public FlashcardStudyService(
        AppDbContext context,
        IFlashcardRepository flashcardRepo,
        IFlashcardDeckRepository deckRepo,
        IUserProgressFlashcardRepository progressRepo,
        IActivityLogRepository logRepo)
    {
        _context = context;
        _flashcardRepo = flashcardRepo;
        _deckRepo = deckRepo;
        _progressRepo = progressRepo;
        _logRepo = logRepo;
    }

    public async Task<StudySessionResponseDto> StartSessionAsync(int userId, StartStudySessionRequestDto request)
    {
        var deck = await _deckRepo.GetByIdAsync(request.DeckId);
        if (deck == null)
            throw new KeyNotFoundException($"Deck {request.DeckId} no encontrado");

        var session = new FlashcardStudySession
        {
            UserId = userId,
            DeckId = request.DeckId,
            SessionType = request.SessionType,
            StartedAt = DateTime.UtcNow,
            CardsReviewed = 0,
            CardsKnown = 0,
            CardsUnknown = 0
        };

        _context.FlashcardStudySessions.Add(session);
        await _context.SaveChangesAsync();

        await _logRepo.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "StartStudySession",
            EntityType = "FlashcardStudySession",
            EntityId = session.Id,
            Details = $"Sesión de estudio iniciada para deck '{deck.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        return new StudySessionResponseDto
        {
            Id = session.Id,
            DeckId = session.DeckId,
            DeckName = deck.Name,
            StartedAt = session.StartedAt,
            EndedAt = null,
            CardsReviewed = 0,
            CardsKnown = 0,
            CardsUnknown = 0,
            SessionType = session.SessionType,
            PerformanceScore = 0
        };
    }

    public async Task<NextCardResponseDto> GetNextCardAsync(int sessionId)
    {
        var session = await _context.FlashcardStudySessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new KeyNotFoundException($"Sesión {sessionId} no encontrada");

        var reviews = await _context.FlashcardReviews
            .Where(r => r.SessionId == sessionId)
            .ToListAsync();

        var reviewedIds = reviews.Select(r => r.FlashcardId).ToHashSet();
        var deckCards = await _flashcardRepo.GetByDeckIdAsync(session.DeckId);
        var remainingCards = deckCards.Where(c => !reviewedIds.Contains(c.Id)).ToList();

        if (remainingCards.Count == 0)
        {
            return new NextCardResponseDto
            {
                SessionId = sessionId,
                Flashcard = null!,
                Current = deckCards.Count(),
                Total = deckCards.Count(),
                RemainingCards = 0
            };
        }

        var nextCard = remainingCards.First();
        var current = reviews.Count + 1;

        return new NextCardResponseDto
        {
            SessionId = sessionId,
            Flashcard = MapToFlashcardDto(nextCard),
            Current = current,
            Total = deckCards.Count(),
            RemainingCards = remainingCards.Count - 1
        };
    }

    public async Task SubmitAnswerAsync(int userId, SubmitFlashcardAnswerRequestDto request)
    {
        var session = await _context.FlashcardStudySessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId);

        if (session == null)
            throw new KeyNotFoundException($"Sesión {request.SessionId} no encontrada");

        var lastReview = (await _flashcardRepo.GetReviewsByUserAndFlashcardAsync(userId, request.FlashcardId))
            .FirstOrDefault();

        var easeFactor = lastReview?.EaseFactor ?? 2.5m;
        var interval = lastReview?.IntervalDays ?? 0;

        if (request.KnewIt)
        {
            interval = interval switch
            {
                0 => 1,
                1 => 6,
                _ => (int)(interval * (double)easeFactor)
            };

            easeFactor += 0.1m;
            if (easeFactor > 3.0m) easeFactor = 3.0m;
        }
        else
        {
            interval = 1;
            easeFactor -= 0.2m;
            if (easeFactor < 1.3m) easeFactor = 1.3m;
        }

        var nextReview = DateTime.UtcNow.Date.AddDays(interval);

        var review = new FlashcardReview
        {
            FlashcardId = request.FlashcardId,
            SessionId = request.SessionId,
            UserId = userId,
            KnewIt = request.KnewIt,
            ResponseTimeMs = request.ResponseTimeMs > 0 ? request.ResponseTimeMs : null,
            EaseFactor = easeFactor,
            IntervalDays = interval,
            NextReviewDate = nextReview,
            ReviewedAt = DateTime.UtcNow
        };

        await _flashcardRepo.AddReviewAsync(review);

        session.CardsReviewed++;
        if (request.KnewIt)
            session.CardsKnown++;
        else
            session.CardsUnknown++;

        _context.FlashcardStudySessions.Update(session);
        await _context.SaveChangesAsync();

        var progress = await _progressRepo.GetByUserAndDeckAsync(userId, session.DeckId);
        if (progress == null)
        {
            progress = new UserProgressFlashcard
            {
                UserId = userId,
                DeckId = session.DeckId
            };
        }

        progress.TotalReviews++;
        progress.AverageEaseFactor = CalculateAverageEaseFactor(userId, session.DeckId);
        var mastery = CalculateDeckMastery(userId, session.DeckId);
        progress.CardsMastered = mastery.Mastered;
        progress.CardsInLearning = mastery.Learning;
        progress.LastStudiedAt = DateTime.UtcNow;
        await _progressRepo.CreateOrUpdateAsync(progress);
    }

    public async Task<StudySessionResponseDto> EndSessionAsync(int userId, EndStudySessionRequestDto request)
    {
        var session = await _context.FlashcardStudySessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId);

        if (session == null)
            throw new KeyNotFoundException($"Sesión {request.SessionId} no encontrada");

        session.EndedAt = DateTime.UtcNow;
        _context.FlashcardStudySessions.Update(session);
        await _context.SaveChangesAsync();

        var deck = await _deckRepo.GetByIdAsync(session.DeckId);
        var performance = session.CardsReviewed > 0
            ? Math.Round((decimal)session.CardsKnown / session.CardsReviewed * 100, 2)
            : 0;

        var progress = await _progressRepo.GetByUserAndDeckAsync(userId, session.DeckId);
        if (progress == null)
        {
            progress = new UserProgressFlashcard
            {
                UserId = userId,
                DeckId = session.DeckId
            };
        }

        progress.TotalStudySessions++;
        progress.LastStudiedAt = DateTime.UtcNow;
        await _progressRepo.CreateOrUpdateAsync(progress);

        await _logRepo.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "EndStudySession",
            EntityType = "FlashcardStudySession",
            EntityId = session.Id,
            Details = $"Sesión finalizada: {session.CardsKnown} conocidas, {session.CardsUnknown} desconocidas",
            CreatedAt = DateTime.UtcNow
        });

        return new StudySessionResponseDto
        {
            Id = session.Id,
            DeckId = session.DeckId,
            DeckName = deck?.Name ?? "",
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            CardsReviewed = session.CardsReviewed,
            CardsKnown = session.CardsKnown,
            CardsUnknown = session.CardsUnknown,
            SessionType = session.SessionType,
            PerformanceScore = performance
        };
    }

    public async Task<DueCardsResponseDto> GetDueCardsAsync(int userId, int deckId)
    {
        var dueReviews = await _flashcardRepo.GetDueReviewsAsync(userId, deckId);
        var now = DateTime.UtcNow.Date;

        var dueCards = dueReviews
            .GroupBy(r => r.FlashcardId)
            .Select(g => g.OrderByDescending(r => r.ReviewedAt).First())
            .Where(r => r.NextReviewDate <= now)
            .ToList();

        var overdue = dueCards.Where(r => r.NextReviewDate < now).ToList();
        var dueToday = dueCards.Where(r => r.NextReviewDate == now).ToList();

        return new DueCardsResponseDto
        {
            DeckId = deckId,
            TotalDue = dueCards.Count,
            OverdueCount = overdue.Count,
            DueTodayCount = dueToday.Count,
            Cards = dueCards.Select(r => MapToFlashcardDto(r.Flashcard!)).ToList()
        };
    }

    public async Task<IEnumerable<FlashcardResponseDto>> GetOverdueCardsAsync(int userId, int deckId)
    {
        var dueReviews = await _flashcardRepo.GetDueReviewsAsync(userId, deckId);
        var now = DateTime.UtcNow.Date;

        var overdue = dueReviews
            .GroupBy(r => r.FlashcardId)
            .Select(g => g.OrderByDescending(r => r.ReviewedAt).First())
            .Where(r => r.NextReviewDate < now)
            .Select(r => r.Flashcard!)
            .Where(f => f != null)
            .Select(MapToFlashcardDto)
            .ToList();

        return overdue;
    }

    public async Task ScheduleReviewAsync(int userId, ScheduleReviewRequestDto request)
    {
        var card = await _flashcardRepo.GetByIdAsync(request.FlashcardId);
        if (card == null)
            throw new KeyNotFoundException($"Flashcard {request.FlashcardId} no encontrada");

        var lastReview = (await _flashcardRepo.GetReviewsByUserAndFlashcardAsync(userId, request.FlashcardId))
            .FirstOrDefault();

        var easeFactor = lastReview?.EaseFactor ?? 2.5m;
        var interval = lastReview?.IntervalDays ?? 0;

        if (request.ForceDate.HasValue)
        {
            interval = (request.ForceDate.Value.Date - DateTime.UtcNow.Date).Days;
            if (interval < 1) interval = 1;
        }
        else
        {
            interval = interval switch
            {
                0 => 1,
                1 => 6,
                _ => (int)(interval * (double)easeFactor)
            };
        }

        var nextReview = DateTime.UtcNow.Date.AddDays(interval);

        // ✅ CORREGIDO: Usar la última sesión activa o crear una virtual
        var lastSession = await _context.FlashcardStudySessions
            .Where(s => s.UserId == userId && s.DeckId == card.DeckId)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        int sessionId;
        if (lastSession != null)
        {
            sessionId = lastSession.Id;
        }
        else
        {
            // Crear una sesión virtual para la revisión programada
            var session = new FlashcardStudySession
            {
                UserId = userId,
                DeckId = card.DeckId,
                SessionType = "repeat_failed",
                StartedAt = DateTime.UtcNow,
                CardsReviewed = 0,
                CardsKnown = 0,
                CardsUnknown = 0
            };
            _context.FlashcardStudySessions.Add(session);
            await _context.SaveChangesAsync();
            sessionId = session.Id;
        }

        var review = new FlashcardReview
        {
            FlashcardId = request.FlashcardId,
            UserId = userId,
            SessionId = sessionId,  // ✅ Ahora usa una sesión real
            KnewIt = true,
            EaseFactor = easeFactor,
            IntervalDays = interval,
            NextReviewDate = nextReview,
            ReviewedAt = DateTime.UtcNow
        };

        await _flashcardRepo.AddReviewAsync(review);

        // Log seguro
        try
        {
            await _logRepo.LogAsync(new ActivityLog
            {
                UserId = userId,
                Action = "ScheduleReview",
                EntityType = "Flashcard",
                EntityId = request.FlashcardId,
                Details = $"Revisión programada para {nextReview:yyyy-MM-dd}",
                CreatedAt = DateTime.UtcNow
            });
        }
        catch { }
    }

    public async Task<IEnumerable<StudySessionResponseDto>> GetSessionHistoryAsync(int userId, int? deckId)
    {
        var query = _context.FlashcardStudySessions
            .Include(s => s.Deck)
            .Where(s => s.UserId == userId);

        if (deckId.HasValue)
            query = query.Where(s => s.DeckId == deckId.Value);

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();

        return sessions.Select(s => new StudySessionResponseDto
        {
            Id = s.Id,
            DeckId = s.DeckId,
            DeckName = s.Deck?.Name ?? "",
            StartedAt = s.StartedAt,
            EndedAt = s.EndedAt,
            CardsReviewed = s.CardsReviewed,
            CardsKnown = s.CardsKnown,
            CardsUnknown = s.CardsUnknown,
            SessionType = s.SessionType,
            PerformanceScore = s.CardsReviewed > 0
                ? Math.Round((decimal)s.CardsKnown / s.CardsReviewed * 100, 2)
                : 0
        });
    }

    public async Task<StudySessionResponseDto> GetSessionAsync(int sessionId)
    {
        var session = await _context.FlashcardStudySessions
            .Include(s => s.Deck)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new KeyNotFoundException($"Sesión {sessionId} no encontrada");

        return new StudySessionResponseDto
        {
            Id = session.Id,
            DeckId = session.DeckId,
            DeckName = session.Deck?.Name ?? "",
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            CardsReviewed = session.CardsReviewed,
            CardsKnown = session.CardsKnown,
            CardsUnknown = session.CardsUnknown,
            SessionType = session.SessionType,
            PerformanceScore = session.CardsReviewed > 0
                ? Math.Round((decimal)session.CardsKnown / session.CardsReviewed * 100, 2)
                : 0
        };
    }

    public async Task<object> GetSessionSummaryAsync(int sessionId)
    {
        var session = await _context.FlashcardStudySessions
            .Include(s => s.Deck)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new KeyNotFoundException($"Sesión {sessionId} no encontrada");

        var reviews = await _context.FlashcardReviews
            .Where(r => r.SessionId == sessionId)
            .ToListAsync();

        var totalTime = reviews.Sum(r => r.ResponseTimeMs ?? 0);
        var avgTimePerCard = reviews.Count > 0 ? totalTime / reviews.Count : 0;

        return new
        {
            SessionId = session.Id,
            DeckName = session.Deck?.Name ?? "",
            Duration = session.EndedAt.HasValue
                ? (session.EndedAt.Value - session.StartedAt).TotalMinutes
                : 0,
            CardsReviewed = session.CardsReviewed,
            CardsKnown = session.CardsKnown,
            CardsUnknown = session.CardsUnknown,
            PerformanceScore = session.CardsReviewed > 0
                ? Math.Round((decimal)session.CardsKnown / session.CardsReviewed * 100, 2)
                : 0,
            TotalResponseTimeMs = totalTime,
            AverageResponseTimePerCardMs = avgTimePerCard
        };
    }

    public async Task<object> GetPerformanceAsync(int userId, int deckId)
    {
        var sessions = await _context.FlashcardStudySessions
            .Where(s => s.UserId == userId && s.DeckId == deckId)
            .ToListAsync();

        var totalReviewed = sessions.Sum(s => s.CardsReviewed);
        var totalKnown = sessions.Sum(s => s.CardsKnown);
        var totalUnknown = sessions.Sum(s => s.CardsUnknown);
        var totalSessions = sessions.Count;

        return new
        {
            DeckId = deckId,
            TotalSessions = totalSessions,
            TotalCardsReviewed = totalReviewed,
            TotalCardsKnown = totalKnown,
            TotalCardsUnknown = totalUnknown,
            OverallPerformance = totalReviewed > 0
                ? Math.Round((decimal)totalKnown / totalReviewed * 100, 2)
                : 0,
            AverageCardsPerSession = totalSessions > 0
                ? Math.Round((double)totalReviewed / totalSessions, 1)
                : 0
        };
    }

    private decimal CalculateAverageEaseFactor(int userId, int deckId)
    {
        var reviews = _context.FlashcardReviews
            .Where(r => r.UserId == userId && r.Flashcard!.DeckId == deckId)
            .ToList();

        if (reviews.Count == 0) return 2.5m;

        var latestPerCard = reviews
            .GroupBy(r => r.FlashcardId)
            .Select(g => g.OrderByDescending(r => r.ReviewedAt).First())
            .ToList();

        return Math.Round(latestPerCard.Average(r => r.EaseFactor), 2);
    }

    private (int Mastered, int Learning) CalculateDeckMastery(int userId, int deckId)
    {
        var latestReviews = _context.FlashcardReviews
            .Where(r => r.UserId == userId && r.Flashcard!.DeckId == deckId)
            .ToList()
            .GroupBy(r => r.FlashcardId)
            .Select(g => g.OrderByDescending(r => r.ReviewedAt).First())
            .ToList();

        var mastered = latestReviews.Count(r => r.KnewIt);
        var learning = latestReviews.Count(r => !r.KnewIt);
        return (mastered, learning);
    }

    private static FlashcardResponseDto MapToFlashcardDto(Flashcard card)
    {
        return new FlashcardResponseDto
        {
            Id = card.Id,
            Question = card.Question,
            Answer = card.Answer,
            Hint = "",
            Difficulty = card.Difficulty,
            Tags = card.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [],
            Position = card.Position,
            IsActive = true,
            DeckId = card.DeckId,
            LastReviewedAt = null,
            NextReviewDate = null,
            RepetitionCount = 0,
            EaseFactor = 2.5m
        };
    }
}
