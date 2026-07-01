using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class CreateIntervalRequestDto
    {
        [Required(ErrorMessage = "El tipo de intervalo es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de intervalo debe tener entre {2} y {1} caracteres.")]
        public string IntervalType { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria.")]
        [Range(1, 1440, ErrorMessage = "La duración debe estar entre {1} y {2} minutos.")]
        public int DurationMinutes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La posición debe ser un valor positivo.")]
        public int OrderPosition { get; set; }
    }
}
