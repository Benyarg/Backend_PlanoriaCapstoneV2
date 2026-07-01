
//NOTE: Falta verificar los endpoints

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using Backend_PlanoriaCapstone.Extensions;

namespace Backend_PlanoriaCapstone.Controllers
{
    [ApiController]
    [Route("api/progress/quizzes")]
    [Authorize]
    public class QuizProgressController : ControllerBase
    {
        private readonly IQuizProgressService _quizProgressService;

        public QuizProgressController(IQuizProgressService quizProgressService)
        {
            _quizProgressService = quizProgressService;
        }

        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetByQuiz(int quizId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.GetByQuizAsync(userId.Value, quizId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.GetByCourseAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOverall()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.GetOverallAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverageScore([FromQuery] int? quizId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.GetAverageScoreAsync(userId.Value, quizId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}/weak-topics")]
        public async Task<IActionResult> GetWeakTopics(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.GetWeakTopicsAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet("improvement")]
        public async Task<IActionResult> GetImprovement([FromQuery] int quizId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.GetImprovementAsync(userId.Value, quizId);
            return Ok(result);
        }

        [HttpGet("compare")]
        public async Task<IActionResult> CompareQuizzes([FromQuery] int quizId1, [FromQuery] int quizId2)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.CompareQuizzesAsync(userId.Value, quizId1, quizId2);
            return Ok(result);
        }

        [HttpGet("compare-courses")]
        public async Task<IActionResult> CompareCourses([FromQuery] int courseId1, [FromQuery] int courseId2)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.CompareCoursesAsync(userId.Value, courseId1, courseId2);
            return Ok(result);
        }

        [HttpGet("compare-timeframes")]
        public async Task<IActionResult> CompareTimeframes(
            [FromQuery] DateTime from1, [FromQuery] DateTime to1,
            [FromQuery] DateTime from2, [FromQuery] DateTime to2)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _quizProgressService.CompareTimeframesAsync(userId.Value, from1, to1, from2, to2);
            return Ok(result);
        }
    }
}
