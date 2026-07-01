using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class QuestionOrderItem
    {
        [Required(ErrorMessage = "El Id de la pregunta es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El Id de la pregunta debe ser un valor positivo.")]
        public int Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La posición debe ser un valor positivo.")]
        public int OrderPosition { get; set; }
    }

    public class ReorderQuestionsRequestDto
    {
        [Required(ErrorMessage = "El orden de las preguntas es obligatorio.")]
        public List<QuestionOrderItem> QuestionOrder { get; set; }
    }
}
