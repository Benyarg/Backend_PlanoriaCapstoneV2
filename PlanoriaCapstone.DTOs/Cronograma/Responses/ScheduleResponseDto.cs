using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Cronograma.Responses
{
    public class ScheduleResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalDurationMinutes { get; set; }
        public List<int> CourseIds { get; set; }
        public List<IntervalResponseDto> Intervals { get; set; }
        public List<ScheduleContentResponseDto> Content { get; set; }
    }
}
