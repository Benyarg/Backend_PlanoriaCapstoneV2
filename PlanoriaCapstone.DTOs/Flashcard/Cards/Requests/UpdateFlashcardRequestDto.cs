using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Cards.Requests
{
    public class UpdateFlashcardRequestDto
    {
        [StringLength(500, MinimumLength = 1, ErrorMessage = "La pregunta debe tener entre 1 y 500 caracteres")]
        public string? Question { get; set; }

        [StringLength(2000, MinimumLength = 1, ErrorMessage = "La respuesta debe tener entre 1 y 2000 caracteres")]
        public string? Answer { get; set; }

        [StringLength(500, ErrorMessage = "La pista no puede exceder 500 caracteres")]
        public string? Hint { get; set; }

        [StringLength(10, ErrorMessage = "La dificultad no puede exceder 10 caracteres")]
        public string? Difficulty { get; set; }

        public List<string>? Tags { get; set; }

        public int? Position { get; set; }

        public bool? IsActive { get; set; }
    }
}
