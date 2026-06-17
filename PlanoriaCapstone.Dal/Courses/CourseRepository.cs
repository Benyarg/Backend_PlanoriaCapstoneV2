using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.FlashcardDecks)
            .Include(c => c.Quizzes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Course>> GetByUserIdAsync(int userId)
    {
        return await _context.Courses
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public async Task<Course> CreateAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
        return course;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return false;

        // 1. Eliminar GeneratedContents directamente
        var generatedContents = await _context.GeneratedContents
            .Where(g => g.CourseId == id)
            .ToListAsync();
        _context.GeneratedContents.RemoveRange(generatedContents);
        await _context.SaveChangesAsync();

        // 2. Eliminar FlashcardReviews
        var deckIds = await _context.FlashcardDecks
            .Where(d => d.CourseId == id)
            .Select(d => d.Id)
            .ToListAsync();

        var cardIds = await _context.Flashcards
            .Where(f => deckIds.Contains(f.DeckId))
            .Select(f => f.Id)
            .ToListAsync();

        var reviews = await _context.FlashcardReviews
            .Where(r => cardIds.Contains(r.FlashcardId))
            .ToListAsync();
        _context.FlashcardReviews.RemoveRange(reviews);

        // 3. Eliminar QuizAnswers y QuizAttempts
        var quizIds = await _context.Quizzes
            .Where(q => q.CourseId == id)
            .Select(q => q.Id)
            .ToListAsync();

        var attemptIds = await _context.QuizAttempts
            .Where(a => quizIds.Contains(a.QuizId))
            .Select(a => a.Id)
            .ToListAsync();

        var answers = await _context.QuizAnswers
            .Where(a => attemptIds.Contains(a.AttemptId))
            .ToListAsync();
        _context.QuizAnswers.RemoveRange(answers);

        var attempts = await _context.QuizAttempts
            .Where(a => quizIds.Contains(a.QuizId))
            .ToListAsync();
        _context.QuizAttempts.RemoveRange(attempts);

        await _context.SaveChangesAsync();

        // 4. Eliminar UserCourses
        var userCourses = await _context.UserCourses
            .Where(u => u.CourseId == id)
            .ToListAsync();
        _context.UserCourses.RemoveRange(userCourses);

        // 5. Eliminar UserCourseExamProgresses
        var examProgress = await _context.UserCourseExamProgresses
            .Where(e => e.CourseId == id)
            .ToListAsync();
        _context.UserCourseExamProgresses.RemoveRange(examProgress);

        // 6. Eliminar ExamReadinessScores
        var readinessScores = await _context.ExamReadinessScores
            .Where(r => r.CourseId == id)
            .ToListAsync();
        _context.ExamReadinessScores.RemoveRange(readinessScores);

        await _context.SaveChangesAsync();

        // 7. Finalmente eliminar el curso
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return true;
    }
}