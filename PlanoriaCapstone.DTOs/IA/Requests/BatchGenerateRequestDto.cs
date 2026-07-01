using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.IA.Requests
{
    public class BatchGenerateRequestDto
    {
        [Required(ErrorMessage = "Los archivos son obligatorios.")]
        public List<int> Files { get; set; }

        [Required(ErrorMessage = "El tipo de contenido es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de contenido debe tener entre {2} y {1} caracteres.")]
        public string ContentType { get; set; }

        [Required(ErrorMessage = "El curso destino es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El curso destino debe ser un valor positivo.")]
        public int TargetCourseId { get; set; }
    }
}
