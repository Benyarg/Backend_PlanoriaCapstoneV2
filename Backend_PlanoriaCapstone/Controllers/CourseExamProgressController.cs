
//NOTE: Falta verificar los endpoints

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using Backend_PlanoriaCapstone.Extensions;

namespace Backend_PlanoriaCapstone.Controllers
{
    [ApiController]
    [Route("api/progress/exam")]
    [Authorize]
    public class CourseExamProgressController : ControllerBase
    {
        private readonly ICourseProgressService _courseProgressService;

        public CourseExamProgressController(ICourseProgressService courseProgressService)
        {
            _courseProgressService = courseProgressService;
        }

        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetExamProgress(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.GetExamProgressAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/readiness")]
        public async Task<IActionResult> GetReadinessScore(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.GetReadinessScoreAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/recommendations")]
        public async Task<IActionResult> GetRecommendations(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.GetRecommendationsAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/readiness/history")]
        public async Task<IActionResult> GetReadinessHistory(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.GetReadinessHistoryAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/readiness/trend")]
        public async Task<IActionResult> GetReadinessTrend(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.GetReadinessTrendAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/predictions")]
        public async Task<IActionResult> GetPredictions(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.GetPredictionsAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/weaknesses")]
        public async Task<IActionResult> IdentifyWeaknesses(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.IdentifyWeaknessesAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/weaknesses/priority")]
        public async Task<IActionResult> GetPriorityTopics(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.GetPriorityTopicsAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/suggest-focus")]
        public async Task<IActionResult> SuggestFocus(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _courseProgressService.SuggestFocusAsync(userId.Value, courseId);
            return Ok(result);
        }
    }
}
