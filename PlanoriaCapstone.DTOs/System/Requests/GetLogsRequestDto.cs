using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.System.Requests
{
    public class GetLogsRequestDto
    {
        [StringLength(50, ErrorMessage = "El nivel no puede superar los {1} caracteres.")]
        public string Level { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        [Range(1, 10000, ErrorMessage = "El límite debe estar entre {1} y {2}.")]
        public int Limit { get; set; } = 100;

        [Range(0, int.MaxValue, ErrorMessage = "El desplazamiento debe ser un valor positivo.")]
        public int Offset { get; set; } = 0;
    }
}
