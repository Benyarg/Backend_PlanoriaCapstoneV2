using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using Backend_PlanoriaCapstone.Extensions;

namespace Backend_PlanoriaCapstone.Controllers
{
    [ApiController]
    [Route("api/flashcards")]
    [Authorize]
    public class FlashcardsController : ControllerBase
    {
        private readonly IFlashcardService _flashcardService;

        public FlashcardsController(IFlashcardService flashcardService)
        {
            _flashcardService = flashcardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByDeck([FromQuery] int? deckId)
        {
            if (!deckId.HasValue)
                return BadRequest(new { message = "deckId es requerido" });

            var result = await _flashcardService.GetByDeckIdAsync(deckId.Value);
            return Ok(result);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.ObtenerUserId();
            if (userId == null) return Unauthorized();

            var result = await _flashcardService.GetAllByUserAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _flashcardService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFlashcardRequestDto request)
        {
            var result = await _flashcardService.CreateAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFlashcardRequestDto request)
        {
            var result = await _flashcardService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _flashcardService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"Flashcard {id} no encontrada" });

            return Ok(new { message = "Flashcard eliminada" });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateFlashcardsRequestDto request)
        {
            var result = await _flashcardService.BulkCreateAsync(request);
            return Ok(result);
        }

        [HttpPut("bulk")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateFlashcardsRequestDto request)
        {
            var result = await _flashcardService.BulkUpdateAsync(request);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchFlashcardRequestDto request)
        {
            var result = await _flashcardService.SearchAsync(request);
            return Ok(result);
        }

        [HttpPost("import/csv")]
        public async Task<IActionResult> ImportCsv([FromQuery] int deckId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Archivo no proporcionado" });

            using var stream = file.OpenReadStream();
            var result = await _flashcardService.ImportFromCsvAsync(deckId, stream);
            return Ok(result);
        }

        [HttpPost("import/json")]
        public async Task<IActionResult> ImportJson([FromQuery] int deckId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Archivo no proporcionado" });

            using var stream = file.OpenReadStream();
            var result = await _flashcardService.ImportFromJsonAsync(deckId, stream);
            return Ok(result);
        }
    }
}
