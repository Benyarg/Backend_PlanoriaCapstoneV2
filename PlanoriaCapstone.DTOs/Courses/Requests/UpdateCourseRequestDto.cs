using System.ComponentModel.DataAnnotations;

public class UpdateCourseRequestDto
{
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
    public string? Name { get; set; } // Agregado '?'

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Description { get; set; } // Agregado '?'

    public DateTime? ExamDate { get; set; }

    [StringLength(10, ErrorMessage = "La hora del examen no puede exceder 10 caracteres")]
    public string? ExamTime { get; set; } // Agregado '?'

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Formato de color inválido. Use el formato #RRGGBB")]
    public string? ColorHex { get; set; } // Agregado '?'

    public bool? IsArchived { get; set; }
}
