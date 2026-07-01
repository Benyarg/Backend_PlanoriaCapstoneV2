namespace PlanoriaCapstone.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "en";
    public string Theme { get; set; } = "light";
    public string Timezone { get; set; } = "UTC";
    public bool NotificationEnabled { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public string? Avatar { get; set; }
    public string DefaultSpacedRepetitionDays { get; set; } = "[1,3,7,14,30]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Course>? Courses { get; set; }
    public ICollection<UserCourse>? UserCourses { get; set; }
    public ICollection<FlashcardStudySession>? FlashcardStudySessions { get; set; }
    public ICollection<FlashcardReview>? FlashcardReviews { get; set; }
    public ICollection<QuizAttempt>? QuizAttempts { get; set; }
    public ICollection<FileUpload>? FileUploads { get; set; }
    public ICollection<StudySchedule>? StudySchedules { get; set; }
    public ICollection<UserProgressFlashcard>? UserProgressFlashcards { get; set; }
    public ICollection<UserProgressQuiz>? UserProgressQuizzes { get; set; }
    public ICollection<UserCourseExamProgress>? UserCourseExamProgresses { get; set; }
    public ICollection<ExamReadinessScore>? ExamReadinessScores { get; set; }
    public ICollection<Notification>? Notifications { get; set; }
    public ICollection<ActivityLog>? ActivityLogs { get; set; }
    public ICollection<SpacedRepetitionSetting>? SpacedRepetitionSettings { get; set; }
    public ICollection<SystemConfiguration>? SystemConfigurations { get; set; }
}
