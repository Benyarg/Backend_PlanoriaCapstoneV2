using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class ScheduleContentRequestDto
    {
        [Required(ErrorMessage = "El ScheduleId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ScheduleId debe ser un valor positivo.")]
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "El tipo de contenido es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de contenido debe tener entre {2} y {1} caracteres.")]
        public string ContentType { get; set; }

        [Required(ErrorMessage = "El ContentId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ContentId debe ser un valor positivo.")]
        public int ContentId { get; set; }

        [Required(ErrorMessage = "Los minutos estimados son obligatorios.")]
        [Range(1, 1440, ErrorMessage = "Los minutos estimados deben estar entre {1} y {2}.")]
        public int EstimatedMinutes { get; set; }

        // ✅ NUEVO: Curso asociado a este contenido
        [Range(1, int.MaxValue, ErrorMessage = "El CourseId debe ser un valor positivo.")]
        public int? CourseId { get; set; }  // Nullable para permitir herencia

        // Opcional: si quieres permitir reordenamiento
        public int? OrderPosition { get; set; }
    }
}