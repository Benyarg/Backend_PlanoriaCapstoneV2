using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.System.Requests
{
    public class UpdateSystemConfigRequestDto
    {
        [Required(ErrorMessage = "La clave de configuración es obligatoria.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "La clave de configuración debe tener entre {2} y {1} caracteres.")]
        public string ConfigKey { get; set; }

        [Required(ErrorMessage = "El valor de configuración es obligatorio.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "El valor de configuración debe tener entre {2} y {1} caracteres.")]
        public string ConfigValue { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        public string Description { get; set; }
    }
}
