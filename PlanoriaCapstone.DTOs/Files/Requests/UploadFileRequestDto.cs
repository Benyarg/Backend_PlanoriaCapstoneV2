using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace PlanoriaCapstone.DTOs.Files.Requests
{
    public class UploadFileRequestDto
    {
        [Required(ErrorMessage = "El archivo es obligatorio.")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "El tipo de archivo es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de archivo debe tener entre {2} y {1} caracteres.")]
        public string FileType { get; set; }

        [Required(ErrorMessage = "El curso es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El curso debe ser un valor positivo.")]
        public int CourseId { get; set; }
    }
}
