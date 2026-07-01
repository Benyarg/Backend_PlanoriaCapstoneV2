using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.IA.Requests
{
    public class GenerateContentRequestDto
    {
        [Required(ErrorMessage = "El FileId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El FileId debe ser un valor positivo.")]
        public int FileId { get; set; }

        [Required(ErrorMessage = "El tipo de contenido es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de contenido debe tener entre {2} y {1} caracteres.")]
        public string ContentType { get; set; }

        [StringLength(500, ErrorMessage = "El tema no puede superar los {1} caracteres.")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "El curso destino es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El curso destino debe ser un valor positivo.")]
        public int TargetCourseId { get; set; }

        [Range(1, 100, ErrorMessage = "El número de elementos debe estar entre {1} y {2}.")]
        public int NumberOfItems { get; set; } = 10;

        [StringLength(50, ErrorMessage = "La dificultad no puede superar los {1} caracteres.")]
        public string Difficulty { get; set; } = "medium";

        [StringLength(10, ErrorMessage = "El idioma no puede superar los {1} caracteres.")]
        public string Language { get; set; } = "es";
    }
}
