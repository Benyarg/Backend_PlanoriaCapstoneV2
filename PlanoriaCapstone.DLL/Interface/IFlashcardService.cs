using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Responses;

namespace PlanoriaCapstone.Bll.Interface;

public interface IFlashcardService
{
    Task<FlashcardResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<FlashcardResponseDto>> GetAllByUserAsync(int userId);
    Task<IEnumerable<FlashcardResponseDto>> GetByDeckIdAsync(int deckId);
    Task<FlashcardResponseDto> CreateAsync(CreateFlashcardRequestDto request);
    Task<FlashcardResponseDto> UpdateAsync(int id, UpdateFlashcardRequestDto request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<FlashcardResponseDto>> BulkCreateAsync(BulkCreateFlashcardsRequestDto request);
    Task<IEnumerable<FlashcardResponseDto>> BulkUpdateAsync(BulkUpdateFlashcardsRequestDto request);
    Task<IEnumerable<FlashcardResponseDto>> SearchAsync(SearchFlashcardRequestDto request);
    Task<IEnumerable<FlashcardResponseDto>> ImportFromCsvAsync(int deckId, Stream csvStream);
    Task<IEnumerable<FlashcardResponseDto>> ImportFromJsonAsync(int deckId, Stream jsonStream);
}
