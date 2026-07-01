using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Cronograma.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class IntervalService : IIntervalService
{
    private readonly IStudyScheduleRepository _scheduleRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public IntervalService(
        IStudyScheduleRepository scheduleRepository,
        IActivityLogRepository activityLogRepository)
    {
        _scheduleRepository = scheduleRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<IntervalResponseDto> CreateAsync(int scheduleId, IntervalResponseDto request)
    {
        var interval = new ScheduleInterval
        {
            ScheduleId = scheduleId,
            IntervalType = request.IntervalType,
            DurationMinutes = request.DurationMinutes,
            OrderPosition = request.OrderPosition
        };

        var created = await _scheduleRepository.AddIntervalAsync(interval);

        return new IntervalResponseDto
        {
            Id = created.Id,
            IntervalType = created.IntervalType,
            DurationMinutes = created.DurationMinutes,
            OrderPosition = created.OrderPosition,
            StartedAt = created.StartedAt,
            EndedAt = created.EndedAt,
            IsCompleted = created.EndedAt.HasValue
        };
    }

    public async Task<IntervalResponseDto> UpdateAsync(int intervalId, IntervalResponseDto request)
    {
        var interval = await _scheduleRepository.GetIntervalByIdAsync(intervalId);
        if (interval == null) throw new KeyNotFoundException();

        interval.IntervalType = request.IntervalType;
        interval.DurationMinutes = request.DurationMinutes;
        interval.OrderPosition = request.OrderPosition;

        await _scheduleRepository.UpdateIntervalAsync(interval);

        return new IntervalResponseDto
        {
            Id = interval.Id,
            IntervalType = interval.IntervalType,
            DurationMinutes = interval.DurationMinutes,
            OrderPosition = interval.OrderPosition,
            StartedAt = interval.StartedAt,
            EndedAt = interval.EndedAt,
            IsCompleted = interval.EndedAt.HasValue
        };
    }

    public async Task<bool> DeleteAsync(int intervalId)
    {
        return await _scheduleRepository.DeleteIntervalAsync(intervalId);
    }

    public async Task ReorderAsync(int scheduleId, List<int> intervalIds)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule?.ScheduleIntervals == null) return;

        foreach (var interval in schedule.ScheduleIntervals)
        {
            var newPos = intervalIds.IndexOf(interval.Id);
            if (newPos >= 0)
                interval.OrderPosition = newPos;
        }

        await _scheduleRepository.UpdateAsync(schedule);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = 0,
            Action = "ReorderIntervals",
            EntityType = "StudySchedule",
            EntityId = scheduleId,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<IntervalResponseDto> GetActiveIntervalAsync(int scheduleId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        var active = schedule?.ScheduleIntervals?
            .FirstOrDefault(i => i.StartedAt.HasValue && !i.EndedAt.HasValue);

        if (active == null)
            throw new InvalidOperationException("No active interval found");

        return new IntervalResponseDto
        {
            Id = active.Id,
            IntervalType = active.IntervalType,
            DurationMinutes = active.DurationMinutes,
            OrderPosition = active.OrderPosition,
            StartedAt = active.StartedAt,
            EndedAt = active.EndedAt,
            IsCompleted = active.EndedAt.HasValue
        };
    }

    public async Task StartTimerAsync(int intervalId)
    {
        var interval = await _scheduleRepository.GetIntervalByIdAsync(intervalId);
        if (interval == null) throw new KeyNotFoundException();

        interval.StartedAt = DateTime.UtcNow;
        await _scheduleRepository.UpdateIntervalAsync(interval);
    }

    public async Task PauseTimerAsync(int intervalId)
    {
        await Task.CompletedTask;
    }

    public async Task ResumeTimerAsync(int intervalId)
    {
        await Task.CompletedTask;
    }

    public async Task StopTimerAsync(int intervalId)
    {
        var interval = await _scheduleRepository.GetIntervalByIdAsync(intervalId);
        if (interval == null) throw new KeyNotFoundException();

        interval.EndedAt = DateTime.UtcNow;
        await _scheduleRepository.UpdateIntervalAsync(interval);
    }

    public async Task<IEnumerable<IntervalResponseDto>> GetTemplatesAsync()
    {
        return new List<IntervalResponseDto>
        {
            new() { Id = 1, IntervalType = "Pomodoro", DurationMinutes = 25, OrderPosition = 1 },
            new() { Id = 2, IntervalType = "ShortBreak", DurationMinutes = 5, OrderPosition = 2 },
            new() { Id = 3, IntervalType = "LongBreak", DurationMinutes = 15, OrderPosition = 3 }
        };
    }

    public async Task<IntervalResponseDto> CreateTemplateAsync(IntervalResponseDto request)
    {
        return await Task.FromResult(new IntervalResponseDto
        {
            Id = new Random().Next(),
            IntervalType = request.IntervalType,
            DurationMinutes = request.DurationMinutes,
            OrderPosition = request.OrderPosition
        });
    }

    public async Task<bool> DeleteTemplateAsync(int templateId)
    {
        return await Task.FromResult(true);
    }

    public async Task ApplyTemplateAsync(int scheduleId, int templateId)
    {
        var templates = await GetTemplatesAsync();
        var template = templates.FirstOrDefault(t => t.Id == templateId);
        if (template == null) throw new KeyNotFoundException();

        await CreateAsync(scheduleId, template);
    }

    private async Task<StudySchedule?> FindScheduleByInterval(int intervalId)
    {
        throw new NotImplementedException("Need repository method to find schedule by interval.");
    }
}
