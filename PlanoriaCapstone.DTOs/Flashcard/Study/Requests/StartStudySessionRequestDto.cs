using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Study.Requests
{
    public class StartStudySessionRequestDto
    {
        [Required(ErrorMessage = "El ID del mazo es requerido")]
        public int DeckId { get; set; }

        [StringLength(20, ErrorMessage = "El tipo de sesión no puede exceder 20 caracteres")]
        public string SessionType { get; set; } = "normal";

        public List<int> IncludeCards { get; set; }
    }
}
