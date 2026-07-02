namespace PlanoriaCapstone.Models;

public class Quiz
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalQuestions { get; set; }
    public decimal PassingScore { get; set; } = 70.00m;
    public int? TimeLimitMinutes { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleOptions { get; set; }
    public int AttemptsAllowed { get; set; }
    public bool ShowResults { get; set; } = true;
    public int? TimePerQuestion { get; set; }
    public bool IsActive { get; set; } = true;  
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Course? Course { get; set; }
    public ICollection<QuizQuestion>? QuizQuestions { get; set; }
    public ICollection<QuizAttempt>? QuizAttempts { get; set; }
    public ICollection<UserProgressQuiz>? UserProgressQuizzes { get; set; }
}
