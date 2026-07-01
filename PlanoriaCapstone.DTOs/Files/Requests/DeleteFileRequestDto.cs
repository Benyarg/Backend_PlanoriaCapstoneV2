using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Files.Requests
{
    public class DeleteFileRequestDto
    {
        [Required(ErrorMessage = "El FileId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El FileId debe ser un valor positivo.")]
        public int FileId { get; set; }

        public bool Permanent { get; set; }
    }
}
