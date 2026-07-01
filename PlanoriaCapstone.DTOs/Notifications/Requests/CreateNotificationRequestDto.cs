using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Notifications.Requests
{
    public class CreateNotificationRequestDto
    {
        [Required(ErrorMessage = "El tipo es obligatorio.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo debe tener entre {2} y {1} caracteres.")]
        public string Type { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "El mensaje debe tener entre {2} y {1} caracteres.")]
        public string Message { get; set; }

        [StringLength(50, ErrorMessage = "El tipo de entidad relacionada no puede superar los {1} caracteres.")]
        public string RelatedEntityType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El Id de entidad relacionada debe ser un valor positivo.")]
        public int? RelatedEntityId { get; set; }

        public DateTime? ScheduledFor { get; set; }
    }
}
