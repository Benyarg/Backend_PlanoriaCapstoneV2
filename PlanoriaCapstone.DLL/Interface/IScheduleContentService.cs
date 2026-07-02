using PlanoriaCapstone.DTOs.Cronograma.Responses;
using PlanoriaCapstone.DTOs.Cronograma.Requests;

namespace PlanoriaCapstone.Bll.Interface;

public interface IScheduleContentService
{
    Task<ScheduleContentResponseDto> AttachContentAsync(ScheduleContentRequestDto request);
    Task<bool> DetachContentAsync(int scheduleId, int contentId);
    Task ReorderContentAsync(int scheduleId, List<int> contentIds);
    Task<IEnumerable<ScheduleContentResponseDto>> GetAssignedContentAsync(int scheduleId);
    Task<int> AutoAssignAsync(int userId, int scheduleId);
    Task<IEnumerable<ScheduleContentResponseDto>> PrioritizeByExamAsync(int userId, int courseId, int scheduleId);
    Task<IEnumerable<ScheduleContentResponseDto>> PrioritizeByWeaknessAsync(int userId, int courseId, int scheduleId);
    Task<object> SuggestSessionAsync(int userId, int courseId);
    Task<IEnumerable<ScheduleContentResponseDto>> SuggestContentAsync(int userId, int scheduleId);
    Task<object> OptimizeScheduleAsync(int userId);
}
