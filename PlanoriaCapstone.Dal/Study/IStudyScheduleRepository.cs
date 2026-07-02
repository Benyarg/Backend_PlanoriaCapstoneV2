using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal;

public interface IStudyScheduleRepository
{
    Task<StudySchedule?> GetByIdAsync(int id);
    Task<IEnumerable<StudySchedule>> GetByUserAsync(int userId);
    Task<IEnumerable<StudySchedule>> GetByDateRangeAsync(int userId, DateTime from, DateTime to);
    Task<StudySchedule> CreateAsync(StudySchedule schedule);
    Task<StudySchedule> UpdateAsync(StudySchedule schedule);
    Task<bool> DeleteAsync(int id);
    Task<ScheduleInterval> AddIntervalAsync(ScheduleInterval interval);
    Task<ScheduleContent> AddContentAsync(ScheduleContent content);
    Task<bool> RemoveContentAsync(int contentId);
    Task<ScheduleInterval?> GetIntervalByIdAsync(int id);
    Task UpdateIntervalAsync(ScheduleInterval interval);
    Task<bool> DeleteIntervalAsync(int id);
}
