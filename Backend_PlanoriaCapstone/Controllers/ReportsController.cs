//NOTE: Falta verificar los endpoints

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Reports.Responses;
using PlanoriaCapstone.DTOs.System.Requests;
using Backend_PlanoriaCapstone.Extensions;

namespace Backend_PlanoriaCapstone.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("study")]
        public async Task<IActionResult> GenerateStudyReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.GenerateStudyReportAsync(userId.Value, from, to);
            return Ok(result);
        }

        [HttpGet("study/insights")]
        public async Task<IActionResult> GetStudyInsights()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.GetStudyInsightsAsync(userId.Value);
            return Ok(result);
        }

        [HttpPost("performance")]
        public async Task<IActionResult> GeneratePerformanceReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.GeneratePerformanceReportAsync(userId.Value, from, to);
            return Ok(result);
        }

        [HttpGet("performance/summary")]
        public async Task<IActionResult> GetPerformanceSummary()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.GetPerformanceSummaryAsync(userId.Value);
            return Ok(result);
        }

        [HttpPost("custom")]
        public async Task<IActionResult> CreateCustomReport([FromBody] CreateCustomReportRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.CreateCustomReportAsync(userId.Value, request);
            return Ok(result);
        }

        [HttpPost("templates")]
        public async Task<IActionResult> SaveTemplate([FromBody] ReportTemplateResponseDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.SaveTemplateAsync(userId.Value, request);
            return Ok(result);
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.GetTemplatesAsync(userId.Value);
            return Ok(result);
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleReport([FromBody] CreateCustomReportRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _reportService.ScheduleReportAsync(userId.Value, request);
            return Ok(result);
        }
    }
}
