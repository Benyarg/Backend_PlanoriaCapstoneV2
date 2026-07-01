using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Auth.Responses
{
    public class VerificationSentResponseDto
    {
        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(500, ErrorMessage = "El mensaje no puede exceder los 500 caracteres")]
        public string Message { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(200, ErrorMessage = "El email no puede exceder los 200 caracteres")]
        public string Email { get; set; }

        public DateTime ResentAt { get; set; }
    }
}