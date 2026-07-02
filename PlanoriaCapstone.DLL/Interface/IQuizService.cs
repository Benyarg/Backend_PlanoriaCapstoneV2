using PlanoriaCapstone.DTOs.Quiz.Requests;
using PlanoriaCapstone.DTOs.Quiz.Responses;

namespace PlanoriaCapstone.Bll.Interface;

public interface IQuizService
{
    Task<QuizResponseDto> GetByIdAsync(int id, int userId);
    Task<IEnumerable<QuizListResponseDto>> GetByCourseIdAsync(int courseId, int userId);
    Task<IEnumerable<QuizListResponseDto>> GetAllAsync(int userId);
    Task<QuizResponseDto> CreateAsync(int userId, CreateQuizRequestDto request);
    Task<QuizResponseDto> UpdateAsync(int id, UpdateQuizRequestDto request);
    Task<bool> DeleteAsync(int id);
    Task<QuizResponseDto> DuplicateAsync(int id, DuplicateQuizRequestDto request);
    Task<IEnumerable<QuestionResponseDto>> GetQuestionsAsync(int quizId);
    Task<QuestionResponseDto> CreateQuestionAsync(int quizId, CreateQuestionRequestDto request);
    Task<QuestionResponseDto> UpdateQuestionAsync(int questionId, UpdateQuestionRequestDto request);
    Task<bool> DeleteQuestionAsync(int questionId);
    Task ReorderQuestionsAsync(int quizId, List<ReorderQuestionsRequestDto> request);
    Task<OptionResponseDto> CreateOptionAsync(int questionId, CreateOptionRequestDto request);
    Task<OptionResponseDto> UpdateOptionAsync(int optionId, UpdateOptionRequestDto request);
    Task<bool> DeleteOptionAsync(int optionId);
    Task UpdateSettingsAsync(int quizId, UpdateQuizSettingsRequestDto settings);
    Task<object> GetSettingsAsync(int quizId);
    Task ResetSettingsAsync(int quizId);
    Task<object> PreviewAsync(int quizId);
    Task<object> SimulateAsync(int quizId);
}
