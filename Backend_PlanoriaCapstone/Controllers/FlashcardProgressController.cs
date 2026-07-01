
//NOTE: Falta verificar los endpoints
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using Backend_PlanoriaCapstone.Extensions;

namespace Backend_PlanoriaCapstone.Controllers
{
    [ApiController]
    [Route("api/progress/flashcards")]
    [Authorize]
    public class FlashcardProgressController : ControllerBase
    {
        private readonly IFlashcardProgressService _flashcardProgressService;

        public FlashcardProgressController(IFlashcardProgressService flashcardProgressService)
        {
            _flashcardProgressService = flashcardProgressService;
        }

        [HttpGet("decks/{deckId}")]
        public async Task<IActionResult> GetByDeck(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetByDeckAsync(userId.Value, deckId);
            return Ok(result);
        }

        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetByCourseAsync(userId.Value, courseId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOverall()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetOverallAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("decks/{deckId}/mastery")]
        public async Task<IActionResult> GetMasteryLevel(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetMasteryLevelAsync(userId.Value, deckId);
            return Ok(result);
        }

        [HttpGet("decks/{deckId}/mastery/trend")]
        public async Task<IActionResult> GetMasteryTrend(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetMasteryTrendAsync(userId.Value, deckId);
            return Ok(result);
        }

        [HttpGet("decks/{deckId}/predictions")]
        public async Task<IActionResult> GetPredictions(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetPredictionsAsync(userId.Value, deckId);
            return Ok(result);
        }

        [HttpGet("decks/{deckId}/timeline")]
        public async Task<IActionResult> GetTimeline(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetTimelineAsync(userId.Value, deckId);
            return Ok(result);
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyProgress()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetWeeklyProgressAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int month, [FromQuery] int year)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();
            var result = await _flashcardProgressService.GetMonthlyReportAsync(userId.Value, month, year);
            return Ok(result);
        }
    }
}
