using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Study.Requests
{
    public class ScheduleReviewRequestDto
    {
        [Required(ErrorMessage = "El ID de la tarjeta es requerido")]
        public int FlashcardId { get; set; }

        public DateTime? ForceDate { get; set; }
    }
}
