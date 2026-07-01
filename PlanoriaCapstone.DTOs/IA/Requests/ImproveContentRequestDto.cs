using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.IA.Requests
{
    public class ImproveContentRequestDto
    {
        [Required(ErrorMessage = "El GeneratedContentId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El GeneratedContentId debe ser un valor positivo.")]
        public int GeneratedContentId { get; set; }

        [StringLength(2000, ErrorMessage = "La retroalimentación no puede superar los {1} caracteres.")]
        public string Feedback { get; set; }

        [StringLength(50, ErrorMessage = "El ajuste de complejidad no puede superar los {1} caracteres.")]
        public string AdjustComplexity { get; set; }

        public List<string> FocusTopics { get; set; }
    }
}
