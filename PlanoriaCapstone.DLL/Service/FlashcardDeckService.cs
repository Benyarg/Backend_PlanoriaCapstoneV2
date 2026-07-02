using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Responses;
using PlanoriaCapstone.DTOs.Flashcards.Decks.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Decks.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class FlashcardDeckService : IFlashcardDeckService
{
    private readonly IFlashcardDeckRepository _deckRepo;
    private readonly IFlashcardRepository _flashcardRepo;
    private readonly IUserProgressFlashcardRepository _progressRepo;
    private readonly IActivityLogRepository _logRepo;

    public FlashcardDeckService(
        IFlashcardDeckRepository deckRepo,
        IFlashcardRepository flashcardRepo,
        IUserProgressFlashcardRepository progressRepo,
        IActivityLogRepository logRepo)
    {
        _deckRepo = deckRepo;
        _flashcardRepo = flashcardRepo;
        _progressRepo = progressRepo;
        _logRepo = logRepo;
    }



    // Gestión de Mazos (Decks) Deck Management (CRUD & Core)
    public async Task<DeckResponseDto> GetByIdAsync(int id)
    {
        var deck = await _deckRepo.GetByIdAsync(id);
        if (deck == null)
            throw new KeyNotFoundException($"Deck {id} no encontrado");

        var userId = deck.Course?.UserId ?? 0;
        return await MapToDeckDto(deck, userId);
    }

    public async Task<IEnumerable<DeckListResponseDto>> GetByUserIdAsync(int userId)
    {
        var decks = await _deckRepo.GetByUserIdAsync(userId);
        var dtos = new List<DeckListResponseDto>();
        foreach (var deck in decks)
            dtos.Add(await MapToListDto(deck, userId));
        return dtos;
    }

    public async Task<IEnumerable<DeckListResponseDto>> GetByCourseIdAsync(int courseId)
    {
        var decks = await _deckRepo.GetByCourseIdAsync(courseId);
        var dtos = new List<DeckListResponseDto>();

        foreach (var deck in decks)
        {
            var userId = deck.Course?.UserId ?? 0;
            dtos.Add(await MapToListDto(deck, userId));
        }

        return dtos;
    }

    public async Task<DeckResponseDto> CreateAsync(int userId, CreateDeckRequestDto request)
    {
        var deck = new FlashcardDeck
        {
            CourseId = request.CourseId,
            Name = request.Name,
            Description = request.Description,
            SpacedRepetitionEnabled = request.SpacedRepetitionEnabled,
            TotalCards = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _deckRepo.CreateAsync(deck);

        await _logRepo.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "CreateDeck",
            EntityType = "FlashcardDeck",
            EntityId = created.Id,
            Details = $"Deck '{created.Name}' creado",
            CreatedAt = DateTime.UtcNow
        });

        return await MapToDeckDto(created, userId);
    }

    public async Task<DeckResponseDto> UpdateAsync(int id, UpdateDeckRequestDto request)
    {
        var deck = await _deckRepo.GetByIdAsync(id);
        if (deck == null)
            throw new KeyNotFoundException($"Deck {id} no encontrado");

        deck.Name = request.Name;
        deck.Description = request.Description;
        if (request.SpacedRepetitionEnabled.HasValue)
            deck.SpacedRepetitionEnabled = request.SpacedRepetitionEnabled.Value;
        deck.UpdatedAt = DateTime.UtcNow;

        var updated = await _deckRepo.UpdateAsync(deck);
        var userId = deck.Course?.UserId ?? 0;
        return await MapToDeckDto(updated, userId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _deckRepo.DeleteAsync(id);
    }

    public async Task<DeckResponseDto> DuplicateAsync(int id, DuplicateDeckRequestDto request)
    {
        var source = await _deckRepo.GetByIdAsync(id);
        if (source == null)
            throw new KeyNotFoundException($"Deck {id} no encontrado");

        var newDeck = new FlashcardDeck
        {
            CourseId = request.TargetCourseId,
            Name = request.NewName ?? source.Name + " (copia)",
            Description = source.Description,
            SpacedRepetitionEnabled = source.SpacedRepetitionEnabled,
            TotalCards = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _deckRepo.CreateAsync(newDeck);

        if (source.Flashcards != null)
        {
            foreach (var card in source.Flashcards)
            {
                var newCard = new Flashcard
                {
                    DeckId = created.Id,
                    Question = card.Question,
                    Answer = card.Answer,
                    Difficulty = card.Difficulty,
                    Tags = card.Tags,
                    Position = card.Position,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _flashcardRepo.CreateAsync(newCard);
            }

            created.TotalCards = source.Flashcards.Count;
            await _deckRepo.UpdateAsync(created);
        }

        var userId = source.Course?.UserId ?? 0;
        return await MapToDeckDto(created, userId);
    }

    public async Task<object> GetStatsAsync(int deckId)
    {
        var deck = await _deckRepo.GetByIdAsync(deckId);
        if (deck == null)
            throw new KeyNotFoundException($"Deck {deckId} no encontrado");

        var cards = await _flashcardRepo.GetByDeckIdAsync(deckId);
        var totalCards = cards.Count();

        return new
        {
            DeckId = deckId,
            TotalCards = totalCards,
            CompletionRate = 0m,
            MasteryLevel = "not_started",
            StudyHeatmap = new { }
        };
    }
    //Flashcard Management (Cards inside Deck)
    public async Task<IEnumerable<FlashcardResponseDto>> GetCardsAsync(int deckId)
    {
        var cards = await _flashcardRepo.GetByDeckIdAsync(deckId);
        return cards.Select(MapToFlashcardDto);
    }

    public async Task AddCardsAsync(int deckId, BulkCreateFlashcardsRequestDto request)
    {
        var deck = await _deckRepo.GetByIdAsync(deckId);
        if (deck == null)
            throw new KeyNotFoundException($"Deck {deckId} no encontrado");

        foreach (var cardReq in request.Cards)
        {
            var card = new Flashcard
            {
                DeckId = deckId,
                Question = cardReq.Question,
                Answer = cardReq.Answer,
                Difficulty = cardReq.Difficulty,
                Tags = cardReq.Tags != null ? string.Join(",", cardReq.Tags) : null,
                Position = cardReq.Position,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _flashcardRepo.CreateAsync(card);
        }

        deck.TotalCards = (await _flashcardRepo.GetByDeckIdAsync(deckId)).Count();
        deck.UpdatedAt = DateTime.UtcNow;
        await _deckRepo.UpdateAsync(deck);
    }

    public async Task RemoveCardsAsync(int deckId, List<int> cardIds)
    {
        foreach (var cardId in cardIds)
        {
            await _flashcardRepo.DeleteAsync(cardId);
        }

        var deck = await _deckRepo.GetByIdAsync(deckId);
        if (deck != null)
        {
            deck.TotalCards = (await _flashcardRepo.GetByDeckIdAsync(deckId)).Count();
            deck.UpdatedAt = DateTime.UtcNow;
            await _deckRepo.UpdateAsync(deck);
        }
    }

    public async Task ReorderCardsAsync(int deckId, ReorderFlashcardsRequestDto request)
    {
        foreach (var item in request.CardOrder)
        {
            var card = await _flashcardRepo.GetByIdAsync(item.Id);
            if (card != null && card.DeckId == deckId)
            {
                card.Position = item.Position;
                card.UpdatedAt = DateTime.UtcNow;
                await _flashcardRepo.UpdateAsync(card);
            }
        }
    }

    //Helper 
    public async Task<int> GetOwnerUserIdAsync(int deckId)
    {
        var deck = await _deckRepo.GetByIdAsync(deckId);
        if (deck == null)
            throw new KeyNotFoundException($"Deck {deckId} no encontrado");
        return deck.Course?.UserId ?? throw new KeyNotFoundException($"No se pudo determinar el propietario del deck {deckId}");
    }

    //Mappers
    private async Task<DeckResponseDto> MapToDeckDto(FlashcardDeck deck, int userId = 0)
    {
        var cards = await _flashcardRepo.GetByDeckIdAsync(deck.Id);
        var totalCards = cards.Count();

        if (userId == 0)
            userId = deck.Course?.UserId ?? 0;

        UserProgressFlashcard? progress = null;
        if (userId > 0)
            progress = await _progressRepo.GetByUserAndDeckAsync(userId, deck.Id);

        var mastered = progress?.CardsMastered ?? 0;
        var learning = progress?.CardsInLearning ?? 0;
        var notStudied = totalCards - mastered - learning;
        if (notStudied < 0) notStudied = 0;

        return new DeckResponseDto
        {
            Id = deck.Id,
            Name = deck.Name,
            Description = deck.Description ?? "",
            CourseId = deck.CourseId,
            CourseName = deck.Course?.Name ?? "",
            TotalCards = totalCards,
            SpacedRepetitionEnabled = deck.SpacedRepetitionEnabled,
            MasteredCards = mastered,
            LearningCards = learning,
            NotStudiedCards = notStudied,
            ProgressPercentage = totalCards > 0 ? Math.Round((decimal)mastered / totalCards * 100, 1) : 0,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        };
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
    private async Task<DeckListResponseDto> MapToListDto(FlashcardDeck deck, int userId)
    {
        UserProgressFlashcard? progress = null;
        if (userId > 0)
            progress = await _progressRepo.GetByUserAndDeckAsync(userId, deck.Id);

        var mastered = progress?.CardsMastered ?? 0;
        var totalCards = deck.TotalCards;
        var masteredPercentage = totalCards > 0 ? Math.Round((decimal)mastered / totalCards * 100, 1) : 0;

        return new DeckListResponseDto
        {
            Id = deck.Id,
            Name = deck.Name,
            CourseName = deck.Course?.Name ?? "",
            ColorHex = deck.Course?.ColorHex ?? "#3498db",
            TotalCards = totalCards,
            MasteredPercentage = masteredPercentage,
            LastStudiedAt = progress?.LastStudiedAt,
            DueCardsCount = progress?.CardsInLearning ?? 0
        };
    }
}
