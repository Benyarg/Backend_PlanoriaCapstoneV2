using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(int id);
    Task<Quiz?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<Quiz>> GetAllAsync();
    Task<IEnumerable<Quiz>> GetAllAsync(int userId);
    Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId, int userId);
    Task<Quiz> CreateAsync(Quiz quiz);
    Task<Quiz> UpdateAsync(Quiz quiz);
    Task<QuizQuestion> AddQuestionAsync(QuizQuestion question);
    Task<bool> DeleteAsync(int id);

    Task UpdateTotalQuestionsAsync(int quizId);
}
