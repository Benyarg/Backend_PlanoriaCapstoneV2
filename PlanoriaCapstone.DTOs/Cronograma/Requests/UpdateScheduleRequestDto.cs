using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class UpdateScheduleRequestDto
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
        public string Title { get; set; }

        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
