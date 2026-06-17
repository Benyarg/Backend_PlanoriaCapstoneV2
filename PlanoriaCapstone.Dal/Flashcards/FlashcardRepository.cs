using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public class FlashcardRepository : IFlashcardRepository
{
    private readonly AppDbContext _context;

    public FlashcardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Flashcard?> GetByIdAsync(int id)
    {
        return await _context.Flashcards
            .Include(f => f.Deck)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
    public async Task<IEnumerable<FlashcardDeck>> GetByUserIdAsync(int userId)
    {
        return await _context.FlashcardDecks
            .Include(d => d.Course)
            .Where(d => d.Course != null && d.Course.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Flashcard>> GetByDeckIdAsync(int deckId)
    {
        return await _context.Flashcards
            .Where(f => f.DeckId == deckId)
            .OrderBy(f => f.Position)
            .ToListAsync();
    }

    public async Task<Flashcard> CreateAsync(Flashcard flashcard)
    {
        _context.Flashcards.Add(flashcard);
        await _context.SaveChangesAsync();
        return flashcard;
    }

    public async Task<Flashcard> UpdateAsync(Flashcard flashcard)
    {
        _context.Flashcards.Update(flashcard);
        await _context.SaveChangesAsync();
        return flashcard;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var flashcard = await _context.Flashcards.FindAsync(id);
        if (flashcard == null) return false;
        _context.Flashcards.Remove(flashcard);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FlashcardReview> AddReviewAsync(FlashcardReview review)
    {
        _context.FlashcardReviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<IEnumerable<FlashcardReview>> GetReviewsByUserAndFlashcardAsync(int userId, int flashcardId)
    {
        return await _context.FlashcardReviews
            .Where(r => r.UserId == userId && r.FlashcardId == flashcardId)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FlashcardReview>> GetDueReviewsAsync(int userId, int deckId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.FlashcardReviews
            .Include(r => r.Flashcard)
            .Where(r => r.UserId == userId
                     && r.Flashcard!.DeckId == deckId
                     && r.NextReviewDate <= today)
            .OrderBy(r => r.NextReviewDate)
            .ToListAsync();
    }
}
