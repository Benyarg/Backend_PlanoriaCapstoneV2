using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class CreateScheduleRequestDto
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha de fin debe ser una fecha válida.")]
        public DateTime EndDateTime { get; set; }

        [Required(ErrorMessage = "Los cursos son obligatorios.")]
        public List<int> CourseIds { get; set; }

        public List<CreateIntervalRequestDto> Intervals { get; set; }
        public List<ScheduleContentRequestDto> Content { get; set; }
    }
}
