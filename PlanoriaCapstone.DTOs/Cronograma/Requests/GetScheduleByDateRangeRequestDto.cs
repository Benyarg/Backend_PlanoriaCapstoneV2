using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class GetScheduleByDateRangeRequestDto
    {
        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El curso debe ser un valor positivo.")]
        public int? CourseId { get; set; }
    }
}
