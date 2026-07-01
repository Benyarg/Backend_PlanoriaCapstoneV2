using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Flashcards.Decks.Responses
{
    public class DeckListResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;  // ✅ NUEVO
        public string ColorHex { get; set; } = "#3498db";       // ✅ NUEVO
        public int TotalCards { get; set; }
        public decimal MasteredPercentage { get; set; }
        public DateTime? LastStudiedAt { get; set; }
        public int DueCardsCount { get; set; }
    }
}