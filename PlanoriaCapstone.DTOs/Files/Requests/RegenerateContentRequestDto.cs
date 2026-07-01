using System.ComponentModel.DataAnnotations;
namespace PlanoriaCapstone.DTOs.Files.Requests
{
    public class RegenerateContentRequestDto
    {
        [Required(ErrorMessage = "El GeneratedContentId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El GeneratedContentId debe ser un valor positivo.")]
        public int GeneratedContentId { get; set; }

        [StringLength(50, ErrorMessage = "El ajuste de complejidad no puede superar los {1} caracteres.")]
        public string AdjustComplexity { get; set; }

        public List<string> FocusOnTopics { get; set; }
    }
}
