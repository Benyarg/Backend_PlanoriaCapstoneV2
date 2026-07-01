using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class GetQuizAttemptsRequestDto
    {
        [Required(ErrorMessage = "El quiz es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El quiz debe ser un valor positivo.")]
        public int QuizId { get; set; }

        [Range(1, 1000, ErrorMessage = "El límite debe estar entre {1} y {2}.")]
        public int? Limit { get; set; }

        [StringLength(50, ErrorMessage = "El campo de ordenamiento no puede superar los {1} caracteres.")]
        public string SortBy { get; set; }
    }
}
