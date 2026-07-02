using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Quiz.Responses
{
    public class QuizListResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int TotalQuestions { get; set; }
        public decimal? BestScore { get; set; }
        public decimal? AverageScore { get; set; }
        public int AttemptsCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
    }
}
