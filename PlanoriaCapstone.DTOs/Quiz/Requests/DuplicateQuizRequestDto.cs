using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class DuplicateQuizRequestDto
    {
        [Required(ErrorMessage = "El nuevo título es obligatorio.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El nuevo título debe tener entre {2} y {1} caracteres.")]
        public string NewTitle { get; set; }

        [Required(ErrorMessage = "El curso destino es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El curso destino debe ser un valor positivo.")]
        public int TargetCourseId { get; set; }
    }
}
