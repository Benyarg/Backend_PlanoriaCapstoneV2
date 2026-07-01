using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Requests
{
    public class ExportDataRequestDto
    {
        [Required(ErrorMessage = "El formato es requerido")]
        [StringLength(10, ErrorMessage = "El formato no puede exceder los 10 caracteres")]
        public string Format { get; set; }

        public bool IncludeFlashcards { get; set; }

        public bool IncludeQuizzes { get; set; }

        public bool IncludeProgress { get; set; }
    }
}