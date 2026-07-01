using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Files.Requests
{
    public class ProcessFileRequestDto
    {
        [Required(ErrorMessage = "El FileId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El FileId debe ser un valor positivo.")]
        public int FileId { get; set; }

        [Required(ErrorMessage = "El formato de contenido es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El formato de contenido debe tener entre {2} y {1} caracteres.")]
        public string ContentFormat { get; set; }

        [StringLength(500, ErrorMessage = "El tema no puede superar los {1} caracteres.")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "El curso destino es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El curso destino debe ser un valor positivo.")]
        public int TargetCourseId { get; set; }

        [StringLength(50, ErrorMessage = "La dificultad no puede superar los {1} caracteres.")]
        public string Difficulty { get; set; }
    }
}
