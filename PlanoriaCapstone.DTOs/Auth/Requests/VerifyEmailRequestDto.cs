using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Auth.Requests
{
    public class VerifyEmailRequestDto
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El token es requerido")]
        [StringLength(500, ErrorMessage = "El token no puede exceder los 500 caracteres")]
        public string Token { get; set; }
    }
}