using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Cronograma.Requests
{
    public class UpdateIntervalRequestDto
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo de intervalo debe tener entre {2} y {1} caracteres.")]
        public string IntervalType { get; set; }

        [Range(1, 1440, ErrorMessage = "La duración debe estar entre {1} y {2} minutos.")]
        public int? DurationMinutes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La posición debe ser un valor positivo.")]
        public int? OrderPosition { get; set; }
    }
}
