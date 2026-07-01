using PlanoriaCapstone.DTOs.Cronograma.Requests;
using PlanoriaCapstone.DTOs.Cronograma.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Interface;

public interface IScheduleService
{
    Task<ScheduleResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<ScheduleListResponseDto>> GetByUserAsync(int userId);
    Task<IEnumerable<StudySchedule>> GetByUserBasicAsync(int userId);
    Task<IEnumerable<ScheduleListResponseDto>> GetByDateRangeAsync(int userId, DateTime from, DateTime to);
    Task<ScheduleResponseDto> CreateAsync(int userId, CreateScheduleRequestDto request);
    Task<ScheduleResponseDto> UpdateAsync(int id, UpdateScheduleRequestDto request);
    Task<bool> DeleteAsync(int id);
    Task<object> GetMonthViewAsync(int userId, int year, int month);
    Task<object> GetWeekViewAsync(int userId, int year, int week);
    Task<object> GetDayViewAsync(int userId, DateTime date);
    Task<object> GetAgendaAsync(int userId, DateTime from, DateTime to);
    Task CreateRecurringAsync(int userId, CreateScheduleRequestDto request, string recurrence);
    Task UpdateRecurringAsync(int scheduleId, UpdateScheduleRequestDto request);
    Task DeleteRecurringAsync(int scheduleId);
    Task MarkCompleteAsync(int scheduleId);
    Task MarkIncompleteAsync(int scheduleId);
    Task BulkCompleteAsync(List<int> scheduleIds);
}
