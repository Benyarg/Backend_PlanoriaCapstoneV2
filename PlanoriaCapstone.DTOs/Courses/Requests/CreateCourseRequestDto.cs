using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Courses.Requests
{
    public class CreateCourseRequestDto
    {
        [Required(ErrorMessage = "El nombre del curso es requerido")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        // Añadí el '?' para permitir que sea null/opcional
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }

        public DateTime? ExamDate { get; set; }

        // Añadí el '?' para permitir que sea null/opcional
        [StringLength(10, ErrorMessage = "La hora del examen no puede exceder 10 caracteres")]
        public string? ExamTime { get; set; }

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Formato de color inválido. Use el formato #RRGGBB")]
        public string ColorHex { get; set; } = "#3498db";
    }
}
