using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Flashcards.Cards.Requests
{
    public class CardOrderItem
    {
        [Required(ErrorMessage = "El ID de la tarjeta es requerido")]
        public int Id { get; set; }

        [Required(ErrorMessage = "La posición es requerida")]
        public int Position { get; set; }
    }

    public class ReorderFlashcardsRequestDto
    {
        [Required(ErrorMessage = "El orden de las tarjetas es requerido")]
        public List<CardOrderItem> CardOrder { get; set; }
    }
}
