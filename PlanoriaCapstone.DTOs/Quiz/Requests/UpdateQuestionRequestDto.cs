using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class UpdateQuestionRequestDto
    {
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "El texto de la pregunta debe tener entre {2} y {1} caracteres.")]
        public string QuestionText { get; set; }

        [StringLength(2000, ErrorMessage = "La explicación no puede superar los {1} caracteres.")]
        public string Explanation { get; set; }

        [Range(0, 1000, ErrorMessage = "Los puntos deben estar entre {1} y {2}.")]
        public decimal? Points { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La posición debe ser un valor positivo.")]
        public int? OrderPosition { get; set; }

        public bool? IsActive { get; set; }

        public List<UpdateOptionRequestDto> Options { get; set; }
    }
}
