using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class FlashcardService : IFlashcardService
{
    private readonly IFlashcardRepository _flashcardRepo;
    private readonly IFlashcardDeckRepository _deckRepo;
    private readonly IActivityLogRepository _logRepo;

    public FlashcardService(
        IFlashcardRepository flashcardRepo,
        IFlashcardDeckRepository deckRepo,
        IActivityLogRepository logRepo)
    {
        _flashcardRepo = flashcardRepo;
        _deckRepo = deckRepo;
        _logRepo = logRepo;
    }

    public async Task<FlashcardResponseDto> GetByIdAsync(int id)
    {
        var card = await _flashcardRepo.GetByIdAsync(id);
        if (card == null)
            throw new KeyNotFoundException($"Flashcard {id} no encontrada");

        return MapToDto(card);
    }
    public async Task<IEnumerable<FlashcardResponseDto>> GetAllByUserAsync(int userId)
    {
        var decks = await _deckRepo.GetByUserIdAsync(userId);
        var allCards = new List<FlashcardResponseDto>();

        foreach (var deck in decks)
        {
            var cards = await _flashcardRepo.GetByDeckIdAsync(deck.Id);
            allCards.AddRange(cards.Select(MapToDto));
        }

        return allCards;
    }

    public async Task<IEnumerable<FlashcardResponseDto>> GetByDeckIdAsync(int deckId)
    {
        var cards = await _flashcardRepo.GetByDeckIdAsync(deckId);
        return cards.Select(MapToDto);
    }

    public async Task<FlashcardResponseDto> CreateAsync(CreateFlashcardRequestDto request)
    {
        var deck = await _deckRepo.GetByIdAsync(request.DeckId);
        if (deck == null)
            throw new KeyNotFoundException($"Deck {request.DeckId} no encontrado");

        var card = new Flashcard
        {
            DeckId = request.DeckId,
            Question = request.Question,
            Answer = request.Answer,
            Difficulty = request.Difficulty,
            Tags = request.Tags != null ? string.Join(",", request.Tags) : null,
            Position = request.Position,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _flashcardRepo.CreateAsync(card);

        deck.TotalCards = (await _flashcardRepo.GetByDeckIdAsync(request.DeckId)).Count();
        deck.UpdatedAt = DateTime.UtcNow;
        await _deckRepo.UpdateAsync(deck);

        return MapToDto(created);
    }

    public async Task<FlashcardResponseDto> UpdateAsync(int id, UpdateFlashcardRequestDto request)
    {
        var card = await _flashcardRepo.GetByIdAsync(id);
        if (card == null)
            throw new KeyNotFoundException($"Flashcard {id} no encontrada");

        card.Question = request.Question;
        card.Answer = request.Answer;
        card.Difficulty = request.Difficulty;
        card.Tags = request.Tags != null ? string.Join(",", request.Tags) : card.Tags;
        if (request.Position.HasValue)
            card.Position = request.Position.Value;
        card.UpdatedAt = DateTime.UtcNow;

        var updated = await _flashcardRepo.UpdateAsync(card);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var card = await _flashcardRepo.GetByIdAsync(id);
        if (card == null)
            return false;

        var result = await _flashcardRepo.DeleteAsync(id);
        if (result)
        {
            var deck = await _deckRepo.GetByIdAsync(card.DeckId);
            if (deck != null)
            {
                deck.TotalCards = (await _flashcardRepo.GetByDeckIdAsync(card.DeckId)).Count();
                deck.UpdatedAt = DateTime.UtcNow;
                await _deckRepo.UpdateAsync(deck);
            }
        }
        return result;
    }

    public async Task<IEnumerable<FlashcardResponseDto>> BulkCreateAsync(BulkCreateFlashcardsRequestDto request)
    {
        var dtos = new List<FlashcardResponseDto>();
        foreach (var item in request.Cards)
        {
            var dto = await CreateAsync(item);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<IEnumerable<FlashcardResponseDto>> BulkUpdateAsync(BulkUpdateFlashcardsRequestDto request)
    {
        var dtos = new List<FlashcardResponseDto>();
        foreach (var item in request.Updates)
        {
            var dto = await UpdateAsync(item.Id, item.Data);
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<IEnumerable<FlashcardResponseDto>> SearchAsync(SearchFlashcardRequestDto request)
    {
        var query = request.Query?.ToLower() ?? "";
        var cards = await _flashcardRepo.GetByDeckIdAsync(request.DeckId ?? 0);

        var filtered = cards.Where(c =>
            (string.IsNullOrEmpty(query) ||
             c.Question.ToLower().Contains(query) ||
             c.Answer.ToLower().Contains(query)) &&
            (string.IsNullOrEmpty(request.Difficulty) || c.Difficulty == request.Difficulty) &&
            (request.Tags == null || request.Tags.Count == 0 ||
             (c.Tags != null && request.Tags.Any(t => c.Tags.Contains(t, StringComparison.OrdinalIgnoreCase))))
        ).Take(request.Limit);

        return filtered.Select(MapToDto);
    }

    public Task<IEnumerable<FlashcardResponseDto>> ImportFromCsvAsync(int deckId, Stream csvStream)
    {
        throw new NotImplementedException("CSV import not yet implemented");
    }

    public Task<IEnumerable<FlashcardResponseDto>> ImportFromJsonAsync(int deckId, Stream jsonStream)
    {
        throw new NotImplementedException("JSON import not yet implemented");
    }

    private static FlashcardResponseDto MapToDto(Flashcard card)
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
