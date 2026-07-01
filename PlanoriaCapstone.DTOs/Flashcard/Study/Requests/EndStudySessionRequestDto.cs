using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Study.Requests
{
    public class EndStudySessionRequestDto
    {
        [Required(ErrorMessage = "El ID de la sesión es requerido")]
        public int SessionId { get; set; }
    }
}
