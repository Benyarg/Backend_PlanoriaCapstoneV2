using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class StartQuizAttemptRequestDto
    {
        [Required(ErrorMessage = "El quiz es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El quiz debe ser un valor positivo.")]
        public int QuizId { get; set; }
    }
}
