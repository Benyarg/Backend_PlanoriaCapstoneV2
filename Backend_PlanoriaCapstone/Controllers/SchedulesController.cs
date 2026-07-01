//Revisado
using Backend_PlanoriaCapstone.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Cronograma.Requests;
using PlanoriaCapstone.DTOs.Cronograma.Responses;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Route("api/schedules")]
    public class SchedulesController : BaseController
    {
        private readonly IScheduleService _scheduleService;
        private readonly ICourseRepository _courseRepository;   // ← Repositorio directo

        public SchedulesController(IScheduleService scheduleService, ICourseRepository courseRepository)
        {
            _scheduleService = scheduleService;
            _courseRepository = courseRepository;
        }

        // CRUD BÁSICO
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleRequestDto request)
        {
            var userId = GetUserId();
            var result = await _scheduleService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetByUser()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var schedules = await _scheduleService.GetByUserBasicAsync(userId.Value);
            var result = new List<ScheduleListResponseDto>();

            foreach (var s in schedules)
            {
                var detail = await _scheduleService.GetByIdAsync(s.Id);
                string courseName = "", colorHex = "#3498db";

                if (detail.CourseIds.Any())
                {
                    // Usamos el repositorio directo (solo necesita el ID)
                    var course = await _courseRepository.GetByIdAsync(detail.CourseIds.First());
                    if (course != null)
                    {
                        courseName = course.Name;
                        colorHex = course.ColorHex;
                    }
                }

                result.Add(new ScheduleListResponseDto
                {
                    Id = detail.Id,
                    Title = detail.Title,
                    StartDateTime = detail.StartDateTime,
                    EndDateTime = detail.EndDateTime,
                    IsCompleted = detail.IsCompleted,
                    ProgressPercentage = detail.IsCompleted ? 100 : 0,
                    CourseName = courseName,
                    ColorHex = colorHex
                });
            }
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (schedule.UserId != userId)
                return Forbidden();
            return Ok(schedule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleRequestDto request)
        {
            var userId = GetUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (schedule.UserId != userId)
                return Forbidden();
            var result = await _scheduleService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (schedule.UserId != userId)
                return Forbidden();
            var result = await _scheduleService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }


        //CALENDARIO
        [HttpGet("calendar/month")]
        public async Task<IActionResult> GetMonthView([FromQuery] int year, [FromQuery] int month)
        {
            var userId = GetUserId();
            var result = await _scheduleService.GetMonthViewAsync(userId, year, month);
            return Ok(result);
        }

        [HttpGet("calendar/week")]
        public async Task<IActionResult> GetWeekView([FromQuery] int year, [FromQuery] int week)
        {
            var userId = GetUserId();
            var result = await _scheduleService.GetWeekViewAsync(userId, year, week);
            return Ok(result);
        }

        [HttpGet("calendar/day")]
        public async Task<IActionResult> GetDayView([FromQuery] DateTime date)
        {
            var userId = GetUserId();
            var result = await _scheduleService.GetDayViewAsync(userId, date);
            return Ok(result);
        }

        [HttpGet("calendar/agenda")]
        public async Task<IActionResult> GetAgenda([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var userId = GetUserId();
            var result = await _scheduleService.GetAgendaAsync(userId, from, to);
            return Ok(result);
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var userId = GetUserId();
            var schedules = await _scheduleService.GetByDateRangeAsync(userId, from, to);
            var result = new List<ScheduleListResponseDto>();

            foreach (var s in schedules)
            {
                var detail = await _scheduleService.GetByIdAsync(s.Id);
                string courseName = "", colorHex = "#3498db";

                if (detail.CourseIds.Any())
                {
                    var course = await _courseRepository.GetByIdAsync(detail.CourseIds.First());
                    if (course != null)
                    {
                        courseName = course.Name;
                        colorHex = course.ColorHex;
                    }
                }

                result.Add(new ScheduleListResponseDto
                {
                    Id = detail.Id,
                    Title = detail.Title,
                    StartDateTime = detail.StartDateTime,
                    EndDateTime = detail.EndDateTime,
                    IsCompleted = detail.IsCompleted,
                    ProgressPercentage = detail.IsCompleted ? 100 : 0,
                    CourseName = courseName,
                    ColorHex = colorHex
                });
            }
            return Ok(result);
        }

        // COMPLETADO
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var userId = GetUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (schedule.UserId != userId)
                return Forbidden();
            await _scheduleService.MarkCompleteAsync(id);
            return Ok(new { message = "Schedule completed" });
        }

        [HttpPatch("{id}/incomplete")]
        public async Task<IActionResult> MarkIncomplete(int id)
        {
            var userId = GetUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (schedule.UserId != userId)
                return Forbidden();
            await _scheduleService.MarkIncompleteAsync(id);
            return Ok(new { message = "Schedule marked incomplete" });
        }

        [HttpPost("bulk-complete")]
        public async Task<IActionResult> BulkComplete([FromBody] List<int> scheduleIds)
        {
            var userId = GetUserId();
            foreach (var id in scheduleIds)
            {
                var schedule = await _scheduleService.GetByIdAsync(id);
                if (schedule == null)
                    return NotFound(new { message = $"Schedule {id} no encontrado" });
                if (schedule.UserId != userId)
                    return Forbidden();
            }
            await _scheduleService.BulkCompleteAsync(scheduleIds);
            return Ok(new { message = "Schedules completed" });
        }

        //  RECURRENCIAS
        [HttpPost("recurring")]
        public async Task<IActionResult> CreateRecurring([FromBody] CreateScheduleRequestDto request, [FromQuery] string recurrence)
        {
            var userId = GetUserId();
            await _scheduleService.CreateRecurringAsync(userId, request, recurrence);
            return Ok(new { message = "Recurring schedules created" });
        }

        [HttpPut("recurring/{id}")]
        public async Task<IActionResult> UpdateRecurring(int id, [FromBody] UpdateScheduleRequestDto request)
        {
            var userId = GetUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (schedule.UserId != userId)
                return Forbidden();
            await _scheduleService.UpdateRecurringAsync(id, request);
            return Ok(new { message = "Recurring schedule updated" });
        }

        [HttpDelete("recurring/{id}")]
        public async Task<IActionResult> DeleteRecurring(int id)
        {
            var userId = GetUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (schedule.UserId != userId)
                return Forbidden();
            await _scheduleService.DeleteRecurringAsync(id);
            return NoContent();
        }

    }
}
