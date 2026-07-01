//Revisado
using Backend_PlanoriaCapstone.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.IA.Requests;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ai")]
    public class AiGenerationController : ControllerBase
    {
        private readonly IAiGenerationService _aiService;

        public AiGenerationController(IAiGenerationService aiService)
        {
            _aiService = aiService;
        }

        //GENERACIÓN DE CONTENIDO
        [HttpPost("generate/flashcards")]
        public async Task<IActionResult> GenerateFlashcards([FromBody] GenerateContentRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _aiService.GenerateFlashcardsAsync(userId.Value, request);
            return Ok(result);
        }

        [HttpPost("generate/quiz")]
        public async Task<IActionResult> GenerateQuiz([FromBody] GenerateContentRequestDto request)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _aiService.GenerateQuizAsync(userId.Value, request);
            return Ok(result);
        }

        [HttpGet("generate/{id}/status")]
        public async Task<IActionResult> GetGenerationStatus(int id)

        {
            var result = await _aiService.GetGenerationStatusAsync(id);
            return Ok(result);
        }

        //MEJORA Y REGENERACIÓN

        [HttpPost("regenerate")]
        public async Task<IActionResult> Regenerate([FromBody] ImproveContentRequestDto request)
        {
            var result = await _aiService.RegenerateAsync(request);
            return Ok(result);
        }

        [HttpPost("improve")]
        public async Task<IActionResult> Improve([FromBody] ImproveContentRequestDto request)
        {
            var result = await _aiService.ImproveQuestionsAsync(request);
            return Ok(result);
        }

        [HttpPost("adjust-difficulty")]
        public async Task<IActionResult> AdjustDifficulty([FromQuery] int generatedContentId, [FromQuery] string difficulty)
        {
            var result = await _aiService.AdjustDifficultyAsync(generatedContentId, difficulty);
            return Ok(result);
        }

        //HISTORIAL
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int? fileId)
        {
            var userId = User.ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            var history = await _aiService.GetHistoryAsync(userId.Value, fileId);
            return Ok(history);
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetGeneratedContent(int id)
        {
            var result = await _aiService.GetGeneratedContentAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpDelete("history/{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var deleted = await _aiService.DeleteHistoryAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        //CONFIGURACIÓN 
        [HttpPut("config")]
        public async Task<IActionResult> SetConfig([FromBody] AIConfigRequestDto request)
        {
            await _aiService.SetConfigAsync(request);
            return Ok(new { message = "Configuración guardada" });
        }

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            var config = await _aiService.GetConfigAsync();
            return Ok(config);
        }

        [HttpPost("config/test")]
        public async Task<IActionResult> TestConnection()
        {
            await _aiService.TestConnectionAsync();
            return Ok(new { message = "Conexión exitosa" });
        }
    }
}
