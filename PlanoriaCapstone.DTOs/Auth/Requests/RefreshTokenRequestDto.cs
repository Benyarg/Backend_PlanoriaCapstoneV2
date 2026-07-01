using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Auth.Requests
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "El token de actualización es requerido")]
        [StringLength(500, ErrorMessage = "El token de actualización no puede exceder los 500 caracteres")]
        public string RefreshToken { get; set; }
    }
}
