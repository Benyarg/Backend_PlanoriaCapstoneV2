using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public interface IFlashcardRepository
{
    Task<Flashcard?> GetByIdAsync(int id);
    Task<IEnumerable<Flashcard>> GetByDeckIdAsync(int deckId);
    Task<Flashcard> CreateAsync(Flashcard flashcard);
    Task<Flashcard> UpdateAsync(Flashcard flashcard);
    Task<bool> DeleteAsync(int id);
    Task<FlashcardReview> AddReviewAsync(FlashcardReview review);
    Task<IEnumerable<FlashcardReview>> GetReviewsByUserAndFlashcardAsync(int userId, int flashcardId);
    Task<IEnumerable<FlashcardReview>> GetDueReviewsAsync(int userId, int deckId);
}