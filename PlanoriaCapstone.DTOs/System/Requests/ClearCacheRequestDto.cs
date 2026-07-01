using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.System.Requests
{
    public class ClearCacheRequestDto
    {
        [Required(ErrorMessage = "El tipo de caché es obligatorio.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El tipo de caché debe tener entre {2} y {1} caracteres.")]
        public string CacheType { get; set; }
    }
}
