using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Requests
{
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre completo debe tener entre 2 y 100 caracteres")]
        public string FullName { get; set; }

        [StringLength(500, ErrorMessage = "La biografía no puede exceder los 500 caracteres")]
        public string? Bio { get; set; }

        [Url(ErrorMessage = "Formato de URL inválido")]
        [StringLength(2048, ErrorMessage = "La URL del avatar no puede exceder los 2048 caracteres")]
        public string? Avatar { get; set; }

        [StringLength(50, ErrorMessage = "La zona horaria no puede exceder los 50 caracteres")]
        public string? Timezone { get; set; }
    }
}