using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Decks.Requests
{
    public class ShareDeckRequestDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El permiso es requerido")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "El permiso debe tener entre 1 y 20 caracteres")]
        public string Permission { get; set; }
    }
}
