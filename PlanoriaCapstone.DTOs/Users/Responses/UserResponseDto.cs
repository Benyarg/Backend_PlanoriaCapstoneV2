using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Responses
{
    public class UserResponseDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a 0")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder los 100 caracteres")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(200, ErrorMessage = "El email no puede exceder los 200 caracteres")]
        public string Email { get; set; }

        [Url(ErrorMessage = "Formato de URL inválido")]
        [StringLength(2048, ErrorMessage = "La URL del avatar no puede exceder los 2048 caracteres")]
        [JsonPropertyName("avatarUrl")]
        public string Avatar { get; set; }

        [StringLength(50, ErrorMessage = "La zona horaria no puede exceder los 50 caracteres")]
        public string Timezone { get; set; }

        [StringLength(10, ErrorMessage = "El idioma no puede exceder los 10 caracteres")]
        public string PreferredLanguage { get; set; }

        [StringLength(20, ErrorMessage = "El tema no puede exceder los 20 caracteres")]
        public string Theme { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}