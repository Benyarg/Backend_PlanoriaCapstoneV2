using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Notifications.Requests
{
    public class ScheduleReminderRequestDto
    {
        [Required(ErrorMessage = "El ScheduleId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ScheduleId debe ser un valor positivo.")]
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "Los minutos de antelación son obligatorios.")]
        [Range(1, 10080, ErrorMessage = "Los minutos de antelación deben estar entre {1} y {2}.")]
        public int RemindMinutesBefore { get; set; }
    }
}
