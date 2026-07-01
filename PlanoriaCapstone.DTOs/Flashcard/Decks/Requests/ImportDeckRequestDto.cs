using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PlanoriaCapstone.DTOs.Flashcards.Decks.Requests
{
    public class ImportDeckRequestDto
    {
        [Required(ErrorMessage = "El formato es requerido")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "El formato debe tener entre 1 y 10 caracteres")]
        public string Format { get; set; }

        [Required(ErrorMessage = "El archivo es requerido")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "El ID del curso destino es requerido")]
        public int TargetCourseId { get; set; }

        public bool ReplaceDuplicates { get; set; }
    }
}
