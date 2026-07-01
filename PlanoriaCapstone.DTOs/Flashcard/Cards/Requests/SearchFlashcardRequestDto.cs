using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Cards.Requests
{
    public class SearchFlashcardRequestDto
    {
        [StringLength(200, ErrorMessage = "La consulta no puede exceder 200 caracteres")]
        public string? Query { get; set; } // Hacemos que sea anulable (string?)

        public int? DeckId { get; set; }

        public List<string>? Tags { get; set; } // Lista anulable

        [StringLength(10, ErrorMessage = "La dificultad no puede exceder 10 caracteres")]
        public string? Difficulty { get; set; }

        public bool? IsActive { get; set; }

        [Range(1, 200, ErrorMessage = "El límite debe estar entre 1 y 200")]
        public int Limit { get; set; } = 50; // Este es el único que siempre tendrá valor
    }
}
