//NOTE: Falta verificar los endpoints

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Flashcards.Study.Requests;
using Backend_PlanoriaCapstone.Extensions;

namespace Backend_PlanoriaCapstone.Controllers
{
    [ApiController]
    [Route("api/study")]
    [Authorize]
    public class StudyController : ControllerBase
    {
        private readonly IFlashcardStudyService _studyService;

        public StudyController(IFlashcardStudyService studyService)
        {
            _studyService = studyService;
        }

        //SESIONES DE ESTUDIO
        [HttpPost("sessions")]
        public async Task<IActionResult> StartSession([FromBody] StartStudySessionRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _studyService.StartSessionAsync(userId.Value, request);
            return Ok(result);
        }

        [HttpGet("sessions/{id}/next")]
        public async Task<IActionResult> GetNextCard(int id)
        {
            var result = await _studyService.GetNextCardAsync(id);
            return Ok(result);
        }

        [HttpPost("sessions/{id}/answer")]
        public async Task<IActionResult> SubmitAnswer(int id, [FromBody] SubmitFlashcardAnswerRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            request.SessionId = id;
            await _studyService.SubmitAnswerAsync(userId.Value, request);
            return Ok(new { message = "Respuesta registrada" });
        }

        [HttpPost("sessions/{id}/end")]
        public async Task<IActionResult> EndSession(int id)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _studyService.EndSessionAsync(userId.Value, new EndStudySessionRequestDto { SessionId = id });
            return Ok(result);
        }


        //CONSULTA DE FLASHCARDS
        [HttpGet("decks/{deckId}/due")]
        public async Task<IActionResult> GetDueCards(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _studyService.GetDueCardsAsync(userId.Value, deckId);
            return Ok(result);
        }

        [HttpGet("decks/{deckId}/overdue")]
        public async Task<IActionResult> GetOverdueCards(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _studyService.GetOverdueCardsAsync(userId.Value, deckId);
            return Ok(result);
        }


        //REVISIONES Y PROGRAMACIÓN
        [HttpPost("reviews/schedule")]
        public async Task<IActionResult> ScheduleReview([FromBody] ScheduleReviewRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            await _studyService.ScheduleReviewAsync(userId.Value, request);
            return Ok(new { message = "Revisión programada" });
        }

        //HISTORIAL Y ESTADISTICAS
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessionHistory([FromQuery] int? deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _studyService.GetSessionHistoryAsync(userId.Value, deckId);
            return Ok(result);
        }

        [HttpGet("sessions/{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            var result = await _studyService.GetSessionAsync(id);
            return Ok(result);
        }

        [HttpGet("sessions/{id}/summary")]
        public async Task<IActionResult> GetSessionSummary(int id)
        {
            var result = await _studyService.GetSessionSummaryAsync(id);
            return Ok(result);
        }

        [HttpGet("decks/{deckId}/performance")]
        public async Task<IActionResult> GetPerformance(int deckId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _studyService.GetPerformanceAsync(userId.Value, deckId);
            return Ok(result);
        }
    }
}
