using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class AutoScheduleRequestDto
    {
        [Required(ErrorMessage = "El curso es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El curso debe ser un valor positivo.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Las horas de estudio por día son obligatorias.")]
        [Range(0.5, 24, ErrorMessage = "Las horas de estudio deben estar entre {1} y {2} horas.")]
        public decimal StudyHoursPerDay { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "La hora de inicio debe tener formato HH:mm.")]
        public string PreferredStartTime { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "La hora de fin debe tener formato HH:mm.")]
        public string PreferredEndTime { get; set; }

        [Required(ErrorMessage = "Los días de la semana son obligatorios.")]
        public List<int> DaysOfWeek { get; set; }

        public bool PrioritizeExam { get; set; }
    }
}
