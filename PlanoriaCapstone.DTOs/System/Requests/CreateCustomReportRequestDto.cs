using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.System.Requests
{
    public class CreateCustomReportRequestDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        public string Name { get; set; }

        public List<string> Filters { get; set; }

        [Required(ErrorMessage = "Las métricas son obligatorias.")]
        public List<string> Metrics { get; set; }

        public List<string> Schedule { get; set; }

        [Required(ErrorMessage = "El formato es obligatorio.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "El formato debe tener entre {2} y {1} caracteres.")]
        public string Format { get; set; }
    }
}
