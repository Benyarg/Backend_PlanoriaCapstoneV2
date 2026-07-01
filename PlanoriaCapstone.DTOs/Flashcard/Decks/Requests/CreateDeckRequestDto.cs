using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Decks.Requests
{
    public class CreateDeckRequestDto
    {
        [Required(ErrorMessage = "El nombre del mazo es requerido")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El ID del curso es requerido")]
        public int CourseId { get; set; }

        public bool SpacedRepetitionEnabled { get; set; } = true;
    }
}
