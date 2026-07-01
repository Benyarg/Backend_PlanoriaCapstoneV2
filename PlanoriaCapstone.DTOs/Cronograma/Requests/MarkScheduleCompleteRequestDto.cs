using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class CompletedContent
    {
        [Required(ErrorMessage = "El tipo de contenido es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de contenido debe tener entre {2} y {1} caracteres.")]
        public string ContentType { get; set; }

        [Required(ErrorMessage = "El ContentId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ContentId debe ser un valor positivo.")]
        public int ContentId { get; set; }

        public bool Completed { get; set; }
    }

    public class MarkScheduleCompleteRequestDto
    {
        [Required(ErrorMessage = "El ScheduleId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ScheduleId debe ser un valor positivo.")]
        public int ScheduleId { get; set; }

        public DateTime? ActualEndTime { get; set; }

        public List<CompletedContent> CompletedContent { get; set; }
    }
}
