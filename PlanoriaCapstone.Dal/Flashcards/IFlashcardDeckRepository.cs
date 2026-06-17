using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public interface IFlashcardDeckRepository
{
    Task<FlashcardDeck?> GetByIdAsync(int id);
    Task<IEnumerable<FlashcardDeck>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<FlashcardDeck>> GetByUserIdAsync(int userId);
    Task<FlashcardDeck> CreateAsync(FlashcardDeck deck);
    Task<FlashcardDeck> UpdateAsync(FlashcardDeck deck);
    Task<bool> DeleteAsync(int id);
}