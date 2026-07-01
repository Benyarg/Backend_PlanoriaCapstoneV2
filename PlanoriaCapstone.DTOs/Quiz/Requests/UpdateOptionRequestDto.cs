using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class UpdateOptionRequestDto
    {
        public int? Id { get; set; } // ✅ AGREGA ESTO

        [StringLength(500, MinimumLength = 1, ErrorMessage = "El texto de la opción debe tener entre {2} y {1} caracteres.")]
        public string OptionText { get; set; }

        public bool? IsCorrect { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La posición debe ser un valor positivo.")]
        public int? OrderPosition { get; set; }
    }
}
