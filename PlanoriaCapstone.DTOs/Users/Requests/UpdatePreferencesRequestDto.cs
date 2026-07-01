using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Requests
{
    public class UpdatePreferencesRequestDto
    {
        [StringLength(20, ErrorMessage = "El tema no puede exceder los 20 caracteres")]
        public string Theme { get; set; }

        [StringLength(10, ErrorMessage = "El idioma no puede exceder los 10 caracteres")]
        public string PreferredLanguage { get; set; }

        public bool? NotificationEnabled { get; set; }

        public bool? EmailNotifications { get; set; }

        public List<int> DefaultSpacedRepetitionDays { get; set; }
    }
}