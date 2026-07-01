using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Requests
{
    public class DeleteAccountRequestDto
    {
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 128 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El texto de confirmación es requerido")]
        [StringLength(100, ErrorMessage = "El texto de confirmación no puede exceder los 100 caracteres")]
        public string ConfirmationText { get; set; }
    }
}