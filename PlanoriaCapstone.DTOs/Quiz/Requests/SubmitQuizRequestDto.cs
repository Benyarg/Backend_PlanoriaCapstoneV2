using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class SubmitQuizRequestDto
    {
        public int AttemptId { get; set; }

        [Required(ErrorMessage = "Las respuestas son obligatorias.")]
        public List<SubmitAnswerRequestDto> Answers { get; set; }
    }
}
