using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Flashcards.Study.Requests
{
    public class GetDueCardsRequestDto
    {
        [Required(ErrorMessage = "El ID del mazo es requerido")]
        public int DeckId { get; set; }

        [Range(1, 200, ErrorMessage = "El límite debe estar entre 1 y 200")]
        public int Limit { get; set; } = 20;

        public bool IncludeOverdue { get; set; } = true;
    }
}