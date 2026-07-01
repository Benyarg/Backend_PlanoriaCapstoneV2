using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class UpdateQuizRequestDto
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
        public string Title { get; set; }

        [StringLength(2000, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        public string Description { get; set; }

        [Range(0, 100, ErrorMessage = "La puntuación de aprobación debe estar entre {1} y {2}.")]
        public decimal? PassingScore { get; set; }

        [Range(1, 600, ErrorMessage = "El límite de tiempo debe estar entre {1} y {2} minutos.")]
        public int? TimeLimitMinutes { get; set; }

        public bool? ShuffleQuestions { get; set; }
        public bool? ShuffleOptions { get; set; }

        [Range(0, 100, ErrorMessage = "Los intentos permitidos deben estar entre {1} y {2}.")]
        public int? AttemptsAllowed { get; set; }

        public bool? IsActive { get; set; }
    }
}
