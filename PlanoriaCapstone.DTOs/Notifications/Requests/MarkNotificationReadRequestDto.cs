using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Notifications.Requests
{
    public class MarkNotificationReadRequestDto
    {
        [Required(ErrorMessage = "La notificación es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La notificación debe ser un valor positivo.")]
        public int NotificationId { get; set; }

        public bool Read { get; set; }
    }
}
