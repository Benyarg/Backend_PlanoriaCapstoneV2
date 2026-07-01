using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Progress.Requests
{
    public class GetProgressRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El curso debe ser un valor positivo.")]
        public int? CourseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El DeckId debe ser un valor positivo.")]
        public int? DeckId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El QuizId debe ser un valor positivo.")]
        public int? QuizId { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
