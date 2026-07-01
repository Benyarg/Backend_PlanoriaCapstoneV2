using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Cards.Requests
{
    public class BulkCreateFlashcardsRequestDto
    {
        [Required(ErrorMessage = "El ID del mazo es requerido")]
        public int DeckId { get; set; }

        [Required(ErrorMessage = "La lista de tarjetas es requerida")]
        public List<CreateFlashcardRequestDto> Cards { get; set; }
    }
}
