//Revisado
using Backend_PlanoriaCapstone.Extensions;
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Route("api/flashcards")]
    public class FlashcardsController : BaseController
    {
        private readonly IFlashcardService _flashcardService;

        public FlashcardsController(IFlashcardService flashcardService)
        {
            _flashcardService = flashcardService;
        }


        // Consultas (Queries)
        [HttpGet]
        public async Task<IActionResult> GetByDeck([FromQuery] int? deckId)
        {
            if (!deckId.HasValue)
                return BadRequest(new { message = "deckId es requerido" });

            var userId = GetUserId();
            var cards = await _flashcardService.GetByDeckIdAsync(deckId.Value);
            return Ok(cards);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var result = await _flashcardService.GetAllByUserAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _flashcardService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Flashcard {id} no encontrada" });
            }

            var result = await _flashcardService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchFlashcardRequestDto request)
        {
            var result = await _flashcardService.SearchAsync(request);
            return Ok(result);
        }


        //Gestión de Datos (Commands)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFlashcardRequestDto request)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _flashcardService.GetDeckOwnerUserIdAsync(request.DeckId);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return BadRequest(new { message = $"Deck {request.DeckId} no encontrado" });
            }

            var result = await _flashcardService.CreateAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFlashcardRequestDto request)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _flashcardService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Flashcard {id} no encontrada" });
            }

            var result = await _flashcardService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _flashcardService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Flashcard {id} no encontrada" });
            }

            var result = await _flashcardService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"Flashcard {id} no encontrada" });

            return Ok(new { message = "Flashcard eliminada" });
        }


        //Operaciones en Lote (Bulk)
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateFlashcardsRequestDto request)
        {
            var userId = GetUserId();
            foreach (var card in request.Cards)
            {
                try
                {
                    var ownerId = await _flashcardService.GetDeckOwnerUserIdAsync(card.DeckId);
                    if (ownerId != userId)
                        return Forbidden();
                }
                catch (KeyNotFoundException)
                {
                    return BadRequest(new { message = $"Deck {card.DeckId} no encontrado" });
                }
            }

            var result = await _flashcardService.BulkCreateAsync(request);
            return Ok(result);
        }

        [HttpPut("bulk")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateFlashcardsRequestDto request)
        {
            var userId = GetUserId();
            foreach (var update in request.Updates)
            {
                try
                {
                    var ownerId = await _flashcardService.GetOwnerUserIdAsync(update.Id);
                    if (ownerId != userId)
                        return Forbidden();
                }
                catch (KeyNotFoundException)
                {
                    return NotFound(new { message = $"Flashcard {update.Id} no encontrada" });
                }
            }

            var result = await _flashcardService.BulkUpdateAsync(request);
            return Ok(result);
        }


        //Importación
        [HttpPost("import/csv")]
        public async Task<IActionResult> ImportCsv([FromQuery] int deckId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Archivo no proporcionado" });

            var userId = GetUserId();
            try
            {
                var ownerId = await _flashcardService.GetDeckOwnerUserIdAsync(deckId);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {deckId} no encontrado" });
            }

            using var stream = file.OpenReadStream();
            var result = await _flashcardService.ImportFromCsvAsync(deckId, stream);
            return Ok(result);
        }

        [HttpPost("import/json")]
        public async Task<IActionResult> ImportJson([FromQuery] int deckId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Archivo no proporcionado" });

            var userId = GetUserId();
            try
            {
                var ownerId = await _flashcardService.GetDeckOwnerUserIdAsync(deckId);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {deckId} no encontrado" });
            }

            using var stream = file.OpenReadStream();
            var result = await _flashcardService.ImportFromJsonAsync(deckId, stream);
            return Ok(result);
        }
    }
}
