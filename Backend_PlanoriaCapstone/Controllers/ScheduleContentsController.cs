//Revisado
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Cronograma.Requests;
using Backend_PlanoriaCapstone.Extensions;

namespace Backend_PlanoriaCapstone.Controllers
{
    [ApiController]
    [Route("api/schedules/{scheduleId}/contents")]
    [Authorize]
    public class ScheduleContentsController : ControllerBase
    {
        private readonly IScheduleContentService _scheduleContentService;
        private readonly IScheduleService _scheduleService;

        public ScheduleContentsController(
            IScheduleContentService scheduleContentService,
            IScheduleService scheduleService)
        {
            _scheduleContentService = scheduleContentService;
            _scheduleService = scheduleService;
        }

        //GESTIÓN DE CONTENIDO ASIGNADO
        [HttpPost]
        public async Task<IActionResult> AttachContent(int scheduleId, [FromBody] ScheduleContentRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            // ✅ Hereda el CourseId si viene del JSON raíz
            request = new ScheduleContentRequestDto
            {
                ContentType = request.ContentType,
                ContentId = request.ContentId,
                EstimatedMinutes = request.EstimatedMinutes,
                ScheduleId = request.ScheduleId > 0 ? request.ScheduleId : scheduleId,
                CourseId = request.CourseId,  // ← Se pasa al servicio
                OrderPosition = request.OrderPosition ?? 0
            };

            var result = await _scheduleContentService.AttachContentAsync(request);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAssignedContent(int scheduleId)
        {

            var result = await _scheduleContentService.GetAssignedContentAsync(scheduleId);
            return Ok(result);
        }

        [HttpPut("reorder")]
        public async Task<IActionResult> ReorderContent(int scheduleId, [FromBody] List<int> contentIds)
        {
            await _scheduleContentService.ReorderContentAsync(scheduleId, contentIds);
            return Ok(new { message = "Content reordered" });
        }

        [HttpDelete]
        public async Task<IActionResult> DetachContent(int scheduleId, [FromQuery] int contentId)
        {
            var result = await _scheduleContentService.DetachContentAsync(scheduleId, contentId);
            if (!result) return NotFound();
            return NoContent();
        }

        //AUTO-ASIGNACIÓN Y PRIORIZACIÓN 
        [HttpPost("auto-assign")]
        public async Task<IActionResult> AutoAssign(int scheduleId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var count = await _scheduleContentService.AutoAssignAsync(userId.Value, scheduleId);
            return Ok(new { assignedCount = count, message = $"{count} contenido(s) asignado(s)" });
        }

        [HttpPost("prioritize-exam")]
        public async Task<IActionResult> PrioritizeByExam(int scheduleId, [FromQuery] int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _scheduleContentService.PrioritizeByExamAsync(userId.Value, courseId, scheduleId);
            return Ok(result);
        }
        [HttpPost("prioritize-weakness")]
        public async Task<IActionResult> PrioritizeByWeakness(int scheduleId, [FromQuery] int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _scheduleContentService.PrioritizeByWeaknessAsync(userId.Value, courseId, scheduleId);
            return Ok(result);
        }


        // SUGERENCIAS Y OPTIMIZACIÓN
        [HttpGet("suggest-session")]
        public async Task<IActionResult> SuggestSession(int scheduleId, [FromQuery] int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _scheduleContentService.SuggestSessionAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("suggest-content")]
        public async Task<IActionResult> SuggestContent(int scheduleId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _scheduleContentService.SuggestContentAsync(userId.Value, scheduleId);
            return Ok(result);
        }

        [HttpGet("optimize")]
        public async Task<IActionResult> OptimizeSchedule()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _scheduleContentService.OptimizeScheduleAsync(userId.Value);
            return Ok(result);
        }
    }
}
