using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Responses;
using PlanoriaCapstone.DTOs.Flashcards.Decks.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Decks.Responses;

namespace PlanoriaCapstone.Bll.Interface;

public interface IFlashcardDeckService
{
    Task<int> GetOwnerUserIdAsync(int deckId);
    Task<DeckResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<DeckListResponseDto>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<DeckListResponseDto>> GetByUserIdAsync(int userId);
    Task<DeckResponseDto> CreateAsync(int userId, CreateDeckRequestDto request);
    Task<DeckResponseDto> UpdateAsync(int id, UpdateDeckRequestDto request);
    Task<bool> DeleteAsync(int id);
    Task<DeckResponseDto> DuplicateAsync(int id, DuplicateDeckRequestDto request);
    Task<IEnumerable<FlashcardResponseDto>> GetCardsAsync(int deckId);
    Task AddCardsAsync(int deckId, BulkCreateFlashcardsRequestDto request);
    Task RemoveCardsAsync(int deckId, List<int> cardIds);
    Task ReorderCardsAsync(int deckId, ReorderFlashcardsRequestDto request);
    Task<object> GetStatsAsync(int deckId);
}
