using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public class FlashcardDeckRepository : IFlashcardDeckRepository
{
    private readonly AppDbContext _context;

    public FlashcardDeckRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FlashcardDeck?> GetByIdAsync(int id)
    {
        return await _context.FlashcardDecks
            .Include(d => d.Flashcards)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
    public async Task<IEnumerable<FlashcardDeck>> GetByUserIdAsync(int userId)
    {
        return await _context.FlashcardDecks
            .Include(d => d.Course)
            .Where(d => d.Course != null && d.Course.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<FlashcardDeck>> GetByCourseIdAsync(int courseId)
    {
        return await _context.FlashcardDecks
            .Where(d => d.CourseId == courseId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<FlashcardDeck> CreateAsync(FlashcardDeck deck)
    {
        _context.FlashcardDecks.Add(deck);
        await _context.SaveChangesAsync();
        return deck;
    }

    public async Task<FlashcardDeck> UpdateAsync(FlashcardDeck deck)
    {
        _context.FlashcardDecks.Update(deck);
        await _context.SaveChangesAsync();
        return deck;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deck = await _context.FlashcardDecks.FindAsync(id);
        if (deck == null) return false;
        _context.FlashcardDecks.Remove(deck);
        await _context.SaveChangesAsync();
        return true;
    }
}
