using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanoriaCapstone.DTOs.Users.Responses;

namespace PlanoriaCapstone.DTOs.Auth.Responses
{
    public class AuthResponseDto
    {
        [Required(ErrorMessage = "El token de acceso es requerido")]
        [StringLength(2048, ErrorMessage = "El token de acceso no puede exceder los 2048 caracteres")]
        public string AccessToken { get; set; }

        [Required(ErrorMessage = "El token de actualización es requerido")]
        [StringLength(500, ErrorMessage = "El token de actualización no puede exceder los 500 caracteres")]
        public string RefreshToken { get; set; }

        [StringLength(50, ErrorMessage = "El tipo de token no puede exceder los 50 caracteres")]
        public string TokenType { get; set; } = "Bearer";

        [Range(0, int.MaxValue, ErrorMessage = "El tiempo de expiración debe ser mayor o igual a 0")]
        public int ExpiresIn { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public UserResponseDto User { get; set; }
    }
}