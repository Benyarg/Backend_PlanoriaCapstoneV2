using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.IA.Requests
{
    public class AIConfigRequestDto
    {
        [Required(ErrorMessage = "El proveedor es obligatorio.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El proveedor debe tener entre {2} y {1} caracteres.")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "La clave API es obligatoria.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "La clave API debe tener entre {2} y {1} caracteres.")]
        public string ApiKey { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El modelo debe tener entre {2} y {1} caracteres.")]
        public string Model { get; set; }

        [Range(1, 100000, ErrorMessage = "Los tokens máximos deben estar entre {1} y {2}.")]
        public int MaxTokens { get; set; } = 2000;

        [Range(0, 2, ErrorMessage = "La temperatura debe estar entre {1} y {2}.")]
        public decimal Temperature { get; set; } = 0.7m;
    }
}
