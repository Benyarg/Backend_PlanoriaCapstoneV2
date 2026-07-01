
//NOTE: Falta verificar los endpoints

using Backend_PlanoriaCapstone.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Quiz.Requests;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/quiz-attempts")]
    public class QuizAttemptsController : ControllerBase
    {
        private readonly IQuizAttemptService _attemptService;

        public QuizAttemptsController(IQuizAttemptService attemptService)
        {
            _attemptService = attemptService;
        }

        //INICIAR Y ENVIAR INTENTO
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartQuizAttemptRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _attemptService.StartAsync(userId.Value, request);
            return Ok(result);
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(int id, [FromBody] SubmitQuizRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            request.AttemptId = id;
            var result = await _attemptService.SubmitAsync(userId.Value, request);
            return Ok(result);
        }


        //• RESPUESTAS.
        [HttpPost("answer")]
        public async Task<IActionResult> SaveAnswer([FromBody] SubmitAnswerRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            await _attemptService.SaveAnswerAsync(userId.Value, request);
            return Ok();
        }

        [HttpPut("answer")]
        public async Task<IActionResult> UpdateAnswer([FromBody] SubmitAnswerRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            await _attemptService.UpdateAnswerAsync(userId.Value, request);
            return Ok();
        }

        [HttpPost("answers/bulk")]
        public async Task<IActionResult> BulkSaveAnswers([FromBody] List<SubmitAnswerRequestDto> request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            await _attemptService.BulkSaveAnswersAsync(userId.Value, request);
            return Ok();
        }

        //CALIFICACIÓN
        [HttpPost("{id}/grade")]
        public async Task<IActionResult> AutoGrade(int id)
        {
            await _attemptService.AutoGradeAsync(id);
            return Ok();
        }

        [HttpPost("{id}/regrade")]
        public async Task<IActionResult> Regrade(int id)
        {
            await _attemptService.RegradeAsync(id);
            return Ok();
        }


        //CONSULTAR INTENTOS
        [HttpGet]
        public async Task<IActionResult> GetAttempts([FromQuery] int? quizId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var attempts = await _attemptService.GetAttemptsAsync(userId.Value, quizId);
            return Ok(attempts);
        }

        [HttpGet("{id}/result")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _attemptService.GetResultAsync(id);
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int quizId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var history = await _attemptService.GetHistoryAsync(userId.Value, quizId);
            return Ok(history);
        }

        [HttpGet("best")]
        public async Task<IActionResult> GetBestAttempt([FromQuery] int quizId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var best = await _attemptService.GetBestAttemptAsync(userId.Value, quizId);
            return Ok(best);
        }

        [HttpGet("compare")]
        public async Task<IActionResult> Compare([FromQuery] string ids)
        {
            var parts = ids?.Split(',');
            if (parts == null || parts.Length != 2 || !int.TryParse(parts[0], out var id1) || !int.TryParse(parts[1], out var id2))
                return BadRequest("Se requieren dos IDs separados por coma (ej: ids=1,2)");

            var result = await _attemptService.CompareAttemptsAsync(id1, id2);
            return Ok(result);
        }
    }
}
