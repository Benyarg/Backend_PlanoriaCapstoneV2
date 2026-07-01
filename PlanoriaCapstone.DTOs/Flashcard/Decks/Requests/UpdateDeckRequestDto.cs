using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Decks.Requests
{
    public class UpdateDeckRequestDto
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Description { get; set; }

        public bool? SpacedRepetitionEnabled { get; set; }

        public bool? IsArchived { get; set; }
    }
}
