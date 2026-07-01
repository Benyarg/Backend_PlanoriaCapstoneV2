using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Progress.Requests
{
    public class SetGoalRequestDto
    {
        [Required(ErrorMessage = "El curso es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El curso debe ser un valor positivo.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "El tipo de objetivo es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de objetivo debe tener entre {2} y {1} caracteres.")]
        public string TargetType { get; set; }

        [Required(ErrorMessage = "El valor objetivo es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El valor objetivo debe ser un valor positivo.")]
        public int TargetValue { get; set; }

        [Required(ErrorMessage = "La fecha límite es obligatoria.")]
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "La métrica es obligatoria.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "La métrica debe tener entre {2} y {1} caracteres.")]
        public string Metric { get; set; }
    }
}
