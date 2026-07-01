using System.ComponentModel.DataAnnotations;

public class SetExamDateRequestDto
{
    [Required(ErrorMessage = "La fecha del examen es requerida")]
    public DateTime ExamDate { get; set; }

    // Añade el '?' para que sea opcional
    [StringLength(10, ErrorMessage = "La hora del examen no puede exceder 10 caracteres")]
    public string? ExamTime { get; set; }

    public bool NotifyMe { get; set; }
}
