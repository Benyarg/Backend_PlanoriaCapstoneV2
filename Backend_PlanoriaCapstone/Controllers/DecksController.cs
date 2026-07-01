//Revisado
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.DTOs.Flashcards.Decks.Requests;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Route("api/decks")]
    public class DecksController : BaseController
    {
        private readonly IFlashcardDeckService _deckService;

        public DecksController(IFlashcardDeckService deckService)
        {
            _deckService = deckService;
        }


        // Gestión de Mazos (Decks) Deck Management (CRUD & Core)

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            var result = await _deckService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyDecks()
        {
            var userId = GetUserId();
            var result = await _deckService.GetByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetByCourse([FromQuery] int? courseId)
        {
            if (!courseId.HasValue)
                return BadRequest(new { message = "courseId es requerido" });

            var result = await _deckService.GetByCourseIdAsync(courseId.Value);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeckRequestDto request)
        {
            var userId = GetUserId();
            var result = await _deckService.CreateAsync(userId, request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDeckRequestDto request)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            var result = await _deckService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            var result = await _deckService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"Deck {id} no encontrado" });

            return Ok(new { message = "Deck eliminado" });
        }

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> Duplicate(int id, [FromBody] DuplicateDeckRequestDto request)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            var result = await _deckService.DuplicateAsync(id, request);
            return Ok(result);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetDeckStats(int id)
        {
            var userId = GetUserId();
            try
            {
                // 1. Validar propiedad
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();

                // 2. Llamar al servicio
                var result = await _deckService.GetStatsAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }
        }

        //Flashcard Management (Cards inside Deck)
        [HttpGet("{id}/cards")]
        public async Task<IActionResult> GetCards(int id)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            var result = await _deckService.GetCardsAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/cards")]
        public async Task<IActionResult> AddCards(int id, [FromBody] BulkCreateFlashcardsRequestDto request)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            await _deckService.AddCardsAsync(id, request);
            return Ok(new { message = "Tarjetas agregadas" });
        }

        //NOTE: no encontrado donde se define el DTO RemoveCardsRequest
        [HttpDelete("{id}/cards")]
        public async Task<IActionResult> RemoveCards(int id, [FromBody] RemoveCardsRequest request)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            await _deckService.RemoveCardsAsync(id, request.CardIds);
            return Ok(new { message = "Tarjetas eliminadas" });
        }

        [HttpPut("{id}/cards/reorder")]
        public async Task<IActionResult> ReorderCards(int id, [FromBody] ReorderFlashcardsRequestDto request)
        {
            var userId = GetUserId();
            try
            {
                var ownerId = await _deckService.GetOwnerUserIdAsync(id);
                if (ownerId != userId)
                    return Forbidden();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Deck {id} no encontrado" });
            }

            await _deckService.ReorderCardsAsync(id, request);
            return Ok(new { message = "Tarjetas reordenadas" });
        }

    }

    public class RemoveCardsRequest
    {
        public List<int> CardIds { get; set; } = new();
    }
}
