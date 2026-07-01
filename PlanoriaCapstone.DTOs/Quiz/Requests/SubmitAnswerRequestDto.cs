using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class SubmitAnswerRequestDto
    {
        public int AttemptId { get; set; }

        [Required(ErrorMessage = "La pregunta es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La pregunta debe ser un valor positivo.")]
        public int QuestionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La opción seleccionada debe ser un valor positivo.")]
        public int? SelectedOptionId { get; set; }

        [StringLength(2000, ErrorMessage = "El texto de respuesta no puede superar los {1} caracteres.")]
        public string ShortAnswerText { get; set; }
    }
}
