using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Auth.Requests
{
    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "La contraseña actual debe tener entre 8 y 128 caracteres")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener entre 8 y 128 caracteres")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmación de la nueva contraseña es requerida")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string NewPasswordConfirmation { get; set; }
    }
}