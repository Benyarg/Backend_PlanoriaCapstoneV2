using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _context;

    public QuizRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetByIdAsync(int id)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Quiz?> GetByIdAsync(int id, int userId)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(q => q.Id == id && q.Course!.UserId == userId);
    }

    public async Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId)
    {
        return await _context.Quizzes
            .Where(q => q.CourseId == courseId)
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId, int userId)
    {
        return await _context.Quizzes
            .Where(q => q.CourseId == courseId && q.Course!.UserId == userId)
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetAllAsync()
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetAllAsync(int userId)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .Where(q => q.Course!.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Quiz> CreateAsync(Quiz quiz)
    {
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }

    public async Task<Quiz> UpdateAsync(Quiz quiz)
    {
        var existingQuiz = await _context.Quizzes
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(q => q.Id == quiz.Id);

        if (existingQuiz != null)
        {
            // Actualizar propiedades simples del quiz
            existingQuiz.Title = quiz.Title;
            existingQuiz.Description = quiz.Description;
            existingQuiz.TotalQuestions = quiz.TotalQuestions;
            existingQuiz.PassingScore = quiz.PassingScore;
            existingQuiz.TimeLimitMinutes = quiz.TimeLimitMinutes;
            existingQuiz.ShuffleQuestions = quiz.ShuffleQuestions;
            existingQuiz.ShuffleOptions = quiz.ShuffleOptions;
            existingQuiz.AttemptsAllowed = quiz.AttemptsAllowed;
            existingQuiz.UpdatedAt = DateTime.UtcNow;

            // ✅ SOLO procesar preguntas si se enviaron
            if (quiz.QuizQuestions != null && quiz.QuizQuestions.Any())
            {
                // Obtener IDs de preguntas existentes en la request
                var updatedQuestionIds = quiz.QuizQuestions
                    .Where(q => q.Id > 0)
                    .Select(q => q.Id)
                    .ToList();

                // Eliminar preguntas que ya no están en la request
                if (existingQuiz.QuizQuestions != null)
                {
                    var questionsToRemove = existingQuiz.QuizQuestions
                        .Where(q => !updatedQuestionIds.Contains(q.Id))
                        .ToList();
                    if (questionsToRemove.Any())
                        _context.QuizQuestions.RemoveRange(questionsToRemove);
                }

                // Procesar cada pregunta
                foreach (var updatedQ in quiz.QuizQuestions)
                {
                    if (updatedQ.Id > 0)
                    {
                        // ✅ Actualizar pregunta existente
                        var existingQ = existingQuiz.QuizQuestions?
                            .FirstOrDefault(q => q.Id == updatedQ.Id);

                        if (existingQ != null)
                        {
                            existingQ.QuestionText = updatedQ.QuestionText;
                            existingQ.QuestionType = updatedQ.QuestionType;
                            existingQ.Explanation = updatedQ.Explanation;
                            existingQ.Points = updatedQ.Points;
                            existingQ.OrderPosition = updatedQ.OrderPosition;
                            existingQ.UpdatedAt = DateTime.UtcNow;

                            // ✅ Procesar opciones de la pregunta
                            if (updatedQ.QuizOptions != null)
                            {
                                var updatedOptionIds = updatedQ.QuizOptions
                                    .Where(o => o.Id > 0)
                                    .Select(o => o.Id)
                                    .ToList();

                                // Eliminar opciones que ya no están
                                if (existingQ.QuizOptions != null)
                                {
                                    var optionsToRemove = existingQ.QuizOptions
                                        .Where(o => !updatedOptionIds.Contains(o.Id))
                                        .ToList();
                                    if (optionsToRemove.Any())
                                        _context.QuizOptions.RemoveRange(optionsToRemove);
                                }

                                // Actualizar o agregar opciones
                                foreach (var optionDto in updatedQ.QuizOptions)
                                {
                                    if (optionDto.Id > 0)
                                    {
                                        // ✅ Actualizar opción existente
                                        var existingOption = existingQ.QuizOptions?
                                            .FirstOrDefault(o => o.Id == optionDto.Id);
                                        if (existingOption != null)
                                        {
                                            existingOption.OptionText = optionDto.OptionText;
                                            existingOption.IsCorrect = optionDto.IsCorrect;
                                            existingOption.OrderPosition = optionDto.OrderPosition;
                                        }
                                    }
                                    else
                                    {
                                        // ✅ Agregar NUEVA opción (Id = 0)
                                        if (existingQ.QuizOptions == null)
                                            existingQ.QuizOptions = new List<QuizOption>();

                                        existingQ.QuizOptions.Add(new QuizOption
                                        {
                                            OptionText = optionDto.OptionText,
                                            IsCorrect = optionDto.IsCorrect,
                                            OrderPosition = optionDto.OrderPosition,
                                            CreatedAt = DateTime.UtcNow
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // ✅ Agregar NUEVA pregunta (Id = 0)
                        if (existingQuiz.QuizQuestions == null)
                            existingQuiz.QuizQuestions = new List<QuizQuestion>();

                        existingQuiz.QuizQuestions.Add(new QuizQuestion
                        {
                            QuestionText = updatedQ.QuestionText,
                            QuestionType = updatedQ.QuestionType,
                            Explanation = updatedQ.Explanation,
                            Points = updatedQ.Points,
                            OrderPosition = updatedQ.OrderPosition,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            QuizOptions = updatedQ.QuizOptions?.Select(o => new QuizOption
                            {
                                OptionText = o.OptionText,
                                IsCorrect = o.IsCorrect,
                                OrderPosition = o.OrderPosition,
                                CreatedAt = DateTime.UtcNow
                            }).ToList()
                        });
                    }
                }
            }
            // ✅ Si no se enviaron preguntas, NO las toques
        }

        await _context.SaveChangesAsync();
        return existingQuiz ?? quiz;
    }
    public async Task<QuizQuestion> AddQuestionAsync(QuizQuestion question)
    {
        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.QuizQuestions!)
                .ThenInclude(qq => qq.QuizOptions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null) return false;

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task UpdateTotalQuestionsAsync(int quizId)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz != null)
        {
            var count = await _context.QuizQuestions
                .Where(q => q.QuizId == quizId)
                .CountAsync();

            quiz.TotalQuestions = count;
            quiz.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
