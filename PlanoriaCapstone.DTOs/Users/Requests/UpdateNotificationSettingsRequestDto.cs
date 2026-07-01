using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Requests
{
    public class UpdateNotificationSettingsRequestDto
    {
        public bool? StudyReminders { get; set; }

        public bool? ExamAlerts { get; set; }

        public bool? AchievementAlerts { get; set; }

        [StringLength(10, ErrorMessage = "La hora del recordatorio no puede exceder los 10 caracteres")]
        public string ReminderTime { get; set; }

        [Range(0, 365, ErrorMessage = "Los días de antelación deben estar entre 0 y 365")]
        public int? ReminderDaysBeforeExam { get; set; }
    }
}