using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Study.Requests
{
    public class SubmitFlashcardAnswerRequestDto
    {
        [Required(ErrorMessage = "El ID de la tarjeta es requerido")]
        public int FlashcardId { get; set; }

        [Required(ErrorMessage = "El ID de la sesión es requerido")]
        public int SessionId { get; set; }

        public bool KnewIt { get; set; }

        [Range(0, 300000, ErrorMessage = "El tiempo de respuesta debe estar entre 0 y 300000 ms")]
        public int ResponseTimeMs { get; set; }
    }
}
