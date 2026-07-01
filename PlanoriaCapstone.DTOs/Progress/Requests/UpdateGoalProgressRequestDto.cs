using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Progress.Requests
{
    public class UpdateGoalProgressRequestDto
    {
        [Required(ErrorMessage = "El GoalId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El GoalId debe ser un valor positivo.")]
        public int GoalId { get; set; }

        [Required(ErrorMessage = "El valor actual es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El valor actual debe ser un valor positivo.")]
        public int CurrentValue { get; set; }

        [StringLength(50, ErrorMessage = "El estado no puede superar los {1} caracteres.")]
        public string Status { get; set; }
    }
}
