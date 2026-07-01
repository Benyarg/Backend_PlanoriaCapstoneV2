using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Decks.Requests
{
    public class DuplicateDeckRequestDto
    {
        [Required(ErrorMessage = "El nuevo nombre es requerido")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
        public string NewName { get; set; }

        [Required(ErrorMessage = "El ID del curso destino es requerido")]
        public int TargetCourseId { get; set; }
    }
}
