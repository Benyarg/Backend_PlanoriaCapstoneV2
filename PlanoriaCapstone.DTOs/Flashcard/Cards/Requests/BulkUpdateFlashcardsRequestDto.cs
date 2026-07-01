using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Cards.Requests
{
    public class FlashcardUpdateItem
    {
        [Required(ErrorMessage = "El ID de la tarjeta es requerido")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Los datos de actualización son requeridos")]
        public UpdateFlashcardRequestDto Data { get; set; }
    }

    public class BulkUpdateFlashcardsRequestDto
    {
        [Required(ErrorMessage = "La lista de actualizaciones es requerida")]
        public List<FlashcardUpdateItem> Updates { get; set; }
    }
}
