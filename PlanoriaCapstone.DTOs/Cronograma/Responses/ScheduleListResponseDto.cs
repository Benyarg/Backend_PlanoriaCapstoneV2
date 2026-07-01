using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Cronograma.Responses
{
    public class ScheduleListResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsCompleted { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#3498db";  // ✅ NUEVO
        public int? CourseId { get; set; }                   // ✅ NUEVO
    }
}