using Microsoft.EntityFrameworkCore;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Dal
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserCourse> UserCourses => Set<UserCourse>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<UserCourseExamProgress> UserCourseExamProgresses => Set<UserCourseExamProgress>();
        public DbSet<ExamReadinessScore> ExamReadinessScores => Set<ExamReadinessScore>();
        public DbSet<FlashcardDeck> FlashcardDecks => Set<FlashcardDeck>();
        public DbSet<Flashcard> Flashcards => Set<Flashcard>();
        public DbSet<FlashcardStudySession> FlashcardStudySessions => Set<FlashcardStudySession>();
        public DbSet<FlashcardReview> FlashcardReviews => Set<FlashcardReview>();
        public DbSet<SpacedRepetitionSetting> SpacedRepetitionSettings => Set<SpacedRepetitionSetting>();
        public DbSet<UserProgressFlashcard> UserProgressFlashcards => Set<UserProgressFlashcard>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
        public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
        public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
        public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();
        public DbSet<UserProgressQuiz> UserProgressQuizzes => Set<UserProgressQuiz>();
        public DbSet<FileUpload> FileUploads => Set<FileUpload>();
        public DbSet<GeneratedContent> GeneratedContents => Set<GeneratedContent>();
        public DbSet<StudySchedule> StudySchedules => Set<StudySchedule>();
        public DbSet<ScheduleInterval> ScheduleIntervals => Set<ScheduleInterval>();
        public DbSet<ScheduleContent> ScheduleContents => Set<ScheduleContent>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ==================================================================
            // USER
            // ==================================================================
            builder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);

                e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
                e.Property(u => u.Email).HasMaxLength(255).IsRequired();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
                e.Property(u => u.PreferredLanguage).HasMaxLength(10).HasDefaultValue("en");
                e.Property(u => u.Theme).HasMaxLength(20).HasDefaultValue("light");
                e.Property(u => u.Timezone).HasMaxLength(50).HasDefaultValue("UTC");
                e.Property(u => u.Avatar).HasMaxLength(500);
                e.Property(u => u.NotificationEnabled).HasDefaultValue(true);
                e.Property(u => u.EmailNotifications).HasDefaultValue(true);
                e.Property(u => u.DefaultSpacedRepetitionDays).HasDefaultValue("[1,3,7,14,30]");
                e.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(u => u.UpdatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(u => u.DeletedAt);
            });

            // ==================================================================
            // USER COURSE
            // ==================================================================
            builder.Entity<UserCourse>(e =>
            {
                e.HasKey(uc => uc.Id);

                e.Property(uc => uc.Role).HasMaxLength(20).HasDefaultValue("owner");
                e.Property(uc => uc.JoinedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(uc => new { uc.UserId, uc.CourseId }).IsUnique();

                e.HasOne(uc => uc.User)
                    .WithMany(u => u.UserCourses)
                    .HasForeignKey(uc => uc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(uc => uc.Course)
                    .WithMany(c => c.UserCourses)
                    .HasForeignKey(uc => uc.CourseId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ==================================================================
            // COURSE
            // ==================================================================
            builder.Entity<Course>(e =>
            {
                e.HasKey(c => c.Id);

                e.Property(c => c.Name).HasMaxLength(200).IsRequired();
                e.Property(c => c.Description);
                e.Property(c => c.ExamDate);
                e.Property(c => c.ExamTime);
                e.Property(c => c.ColorHex).HasMaxLength(7).HasDefaultValue("#3498db");
                e.Property(c => c.IsArchived).HasDefaultValue(false);
                e.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(c => c.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(c => c.UserId);
                e.HasIndex(c => c.ExamDate);

                e.HasOne(c => c.User)
                    .WithMany(u => u.Courses)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // USER COURSE EXAM PROGRESS
            // ==================================================================
            builder.Entity<UserCourseExamProgress>(e =>
            {
                e.HasKey(p => p.Id);

                e.Property(p => p.TotalFlashcards).HasDefaultValue(0);
                e.Property(p => p.FlashcardsStudied).HasDefaultValue(0);
                e.Property(p => p.FlashcardsMastered).HasDefaultValue(0);
                e.Property(p => p.TotalQuizzes).HasDefaultValue(0);
                e.Property(p => p.QuizzesCompleted).HasDefaultValue(0);
                e.Property(p => p.QuizzesPassed).HasDefaultValue(0);
                e.Property(p => p.ExamReadinessScore).HasPrecision(5, 2).HasDefaultValue(0);
                e.Property(p => p.LastCalculatedAt);
                e.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(p => new { p.UserId, p.CourseId }).IsUnique();

                e.HasOne(p => p.User)
                    .WithMany(u => u.UserCourseExamProgresses)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.Course)
                    .WithMany(c => c.UserCourseExamProgresses)
                    .HasForeignKey(p => p.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // EXAM READINESS SCORE
            // ==================================================================
            builder.Entity<ExamReadinessScore>(e =>
            {
                e.HasKey(s => s.Id);

                e.Property(s => s.Score).HasPrecision(5, 2).IsRequired();
                e.Property(s => s.DaysUntilExam);
                e.Property(s => s.CalculatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(s => new { s.UserId, s.CourseId, s.CalculatedAt });

                e.HasOne(s => s.User)
                    .WithMany(u => u.ExamReadinessScores)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(s => s.Course)
                    .WithMany(c => c.ExamReadinessScores)
                    .HasForeignKey(s => s.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // FLASHCARD DECK
            // ==================================================================
            builder.Entity<FlashcardDeck>(e =>
            {
                e.HasKey(d => d.Id);

                e.Property(d => d.Name).HasMaxLength(200).IsRequired();
                e.Property(d => d.Description);
                e.Property(d => d.TotalCards).HasDefaultValue(0);
                e.Property(d => d.SpacedRepetitionEnabled).HasDefaultValue(true);
                e.Property(d => d.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(d => d.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(d => d.CourseId);

                e.HasOne(d => d.Course)
                    .WithMany(c => c.FlashcardDecks)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // FLASHCARD
            // ==================================================================
            builder.Entity<Flashcard>(e =>
            {
                e.HasKey(f => f.Id);

                e.Property(f => f.Question).IsRequired();
                e.Property(f => f.Answer).IsRequired();
                e.Property(f => f.Difficulty).HasMaxLength(10).HasDefaultValue("medium");
                e.Property(f => f.Tags);
                e.Property(f => f.Position).HasDefaultValue(0);
                e.Property(f => f.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(f => f.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(f => f.DeckId);
                e.HasIndex(f => f.Difficulty);

                e.HasOne(f => f.Deck)
                    .WithMany(d => d.Flashcards)
                    .HasForeignKey(f => f.DeckId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // FLASHCARD STUDY SESSION
            // ==================================================================
            builder.Entity<FlashcardStudySession>(e =>
            {
                e.HasKey(s => s.Id);

                e.Property(s => s.StartedAt).IsRequired();
                e.Property(s => s.CardsReviewed).HasDefaultValue(0);
                e.Property(s => s.CardsKnown).HasDefaultValue(0);
                e.Property(s => s.CardsUnknown).HasDefaultValue(0);
                e.Property(s => s.SessionType).HasMaxLength(20).HasDefaultValue("normal");

                e.HasIndex(s => new { s.UserId, s.DeckId, s.StartedAt });

                e.HasOne(s => s.User)
                    .WithMany(u => u.FlashcardStudySessions)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(s => s.Deck)
                    .WithMany(d => d.FlashcardStudySessions)
                    .HasForeignKey(s => s.DeckId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // FLASHCARD REVIEW
            // ==================================================================
            builder.Entity<FlashcardReview>(e =>
            {
                e.HasKey(r => r.Id);

                e.Property(r => r.KnewIt).IsRequired();
                e.Property(r => r.ResponseTimeMs);
                e.Property(r => r.EaseFactor).HasPrecision(4, 2).HasDefaultValue(2.5m);
                e.Property(r => r.IntervalDays).HasDefaultValue(1);
                e.Property(r => r.NextReviewDate).IsRequired();
                e.Property(r => r.ReviewedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(r => new { r.UserId, r.NextReviewDate });
                e.HasIndex(r => new { r.FlashcardId, r.UserId });

                e.HasOne(r => r.Flashcard)
                    .WithMany(f => f.FlashcardReviews)
                    .HasForeignKey(r => r.FlashcardId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(r => r.Session)
                    .WithMany(s => s.FlashcardReviews)
                    .HasForeignKey(r => r.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(r => r.User)
                    .WithMany(u => u.FlashcardReviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ==================================================================
            // SPACED REPETITION SETTING
            // ==================================================================
            builder.Entity<SpacedRepetitionSetting>(e =>
            {
                e.HasKey(s => s.Id);

                e.Property(s => s.InitialIntervalDays).HasDefaultValue(1);
                e.Property(s => s.MaxIntervalDays).HasDefaultValue(365);
                e.Property(s => s.EasyBonus).HasPrecision(3, 2).HasDefaultValue(1.30m);
                e.Property(s => s.HardPenalty).HasPrecision(3, 2).HasDefaultValue(1.20m);
                e.Property(s => s.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(s => s.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(s => new { s.UserId, s.DeckId }).IsUnique().HasFilter("[DeckId] IS NOT NULL");

                e.HasOne(s => s.User)
                    .WithMany(u => u.SpacedRepetitionSettings)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(s => s.Deck)
                    .WithMany(d => d.SpacedRepetitionSettings)
                    .HasForeignKey(s => s.DeckId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ==================================================================
            // USER PROGRESS FLASHCARD
            // ==================================================================
            builder.Entity<UserProgressFlashcard>(e =>
            {
                e.HasKey(p => p.Id);

                e.Property(p => p.TotalStudySessions).HasDefaultValue(0);
                e.Property(p => p.TotalReviews).HasDefaultValue(0);
                e.Property(p => p.CardsMastered).HasDefaultValue(0);
                e.Property(p => p.CardsInLearning).HasDefaultValue(0);
                e.Property(p => p.AverageEaseFactor).HasPrecision(4, 2).HasDefaultValue(2.50m);
                e.Property(p => p.LastStudiedAt);
                e.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(p => new { p.UserId, p.DeckId }).IsUnique();

                e.HasOne(p => p.User)
                    .WithMany(u => u.UserProgressFlashcards)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.Deck)
                    .WithMany(d => d.UserProgressFlashcards)
                    .HasForeignKey(p => p.DeckId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // QUIZ
            // ==================================================================
            builder.Entity<Quiz>(e =>
            {
                e.HasKey(q => q.Id);

                e.Property(q => q.Title).HasMaxLength(200).IsRequired();
                e.Property(q => q.Description);
                e.Property(q => q.TotalQuestions).HasDefaultValue(0);
                e.Property(q => q.PassingScore).HasPrecision(5, 2).HasDefaultValue(70.00m);
                e.Property(q => q.TimeLimitMinutes);
                e.Property(q => q.ShuffleQuestions).HasDefaultValue(false);
                e.Property(q => q.ShuffleOptions).HasDefaultValue(false);
                e.Property(q => q.AttemptsAllowed).HasDefaultValue(0);
                e.Property(q => q.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(q => q.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(q => q.CourseId);

                e.HasOne(q => q.Course)
                    .WithMany(c => c.Quizzes)
                    .HasForeignKey(q => q.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // QUIZ QUESTION
            // ==================================================================
            builder.Entity<QuizQuestion>(e =>
            {
                e.HasKey(q => q.Id);

                e.Property(q => q.QuestionText).IsRequired();
                e.Property(q => q.QuestionType).HasMaxLength(20).IsRequired();
                e.Property(q => q.Explanation);
                e.Property(q => q.Points).HasPrecision(5, 2).HasDefaultValue(1.00m);
                e.Property(q => q.OrderPosition).HasDefaultValue(0);
                e.Property(q => q.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(q => q.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(q => q.QuizId);

                e.HasOne(q => q.Quiz)
                    .WithMany(qz => qz.QuizQuestions)
                    .HasForeignKey(q => q.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // QUIZ OPTION
            // ==================================================================
            builder.Entity<QuizOption>(e =>
            {
                e.HasKey(o => o.Id);

                e.Property(o => o.OptionText).IsRequired();
                e.Property(o => o.IsCorrect).IsRequired();
                e.Property(o => o.OrderPosition).HasDefaultValue(0);
                e.Property(o => o.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(o => o.QuestionId);

                e.HasOne(o => o.Question)
                    .WithMany(q => q.QuizOptions)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // QUIZ ATTEMPT
            // ==================================================================
            builder.Entity<QuizAttempt>(e =>
            {
                e.HasKey(a => a.Id);

                e.Property(a => a.StartedAt).IsRequired();
                e.Property(a => a.ScorePercentage).HasPrecision(5, 2);
                e.Property(a => a.Passed);
                e.Property(a => a.TimeSpentSeconds);
                e.Property(a => a.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(a => new { a.UserId, a.QuizId });

                e.HasOne(a => a.User)
                    .WithMany(u => u.QuizAttempts)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(a => a.Quiz)
                    .WithMany(q => q.QuizAttempts)
                    .HasForeignKey(a => a.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // QUIZ ANSWER
            // ==================================================================
            builder.Entity<QuizAnswer>(e =>
            {
                e.HasKey(a => a.Id);

                e.Property(a => a.ShortAnswerText);
                e.Property(a => a.IsCorrect).IsRequired();
                e.Property(a => a.PointsEarned).HasPrecision(5, 2).HasDefaultValue(0);
                e.Property(a => a.AnsweredAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(a => a.AttemptId);
                e.HasIndex(a => a.QuestionId);

                e.HasOne(a => a.Attempt)
                    .WithMany(at => at.QuizAnswers)
                    .HasForeignKey(a => a.AttemptId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.Question)
                    .WithMany(q => q.QuizAnswers)
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(a => a.SelectedOption)
                    .WithMany(o => o.QuizAnswers)
                    .HasForeignKey(a => a.SelectedOptionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ==================================================================
            // USER PROGRESS QUIZ
            // ==================================================================
            builder.Entity<UserProgressQuiz>(e =>
            {
                e.HasKey(p => p.Id);

                e.Property(p => p.TotalAttempts).HasDefaultValue(0);
                e.Property(p => p.BestScore).HasPrecision(5, 2).HasDefaultValue(0);
                e.Property(p => p.AverageScore).HasPrecision(5, 2).HasDefaultValue(0);
                e.Property(p => p.LastAttemptAt);
                e.Property(p => p.PassedCount).HasDefaultValue(0);
                e.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(p => new { p.UserId, p.QuizId }).IsUnique();

                e.HasOne(p => p.User)
                    .WithMany(u => u.UserProgressQuizzes)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.Quiz)
                    .WithMany(q => q.UserProgressQuizzes)
                    .HasForeignKey(p => p.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // FILE UPLOAD
            // ==================================================================
            builder.Entity<FileUpload>(e =>
            {
                e.HasKey(f => f.Id);

                e.Property(f => f.OriginalFilename).HasMaxLength(255).IsRequired();
                e.Property(f => f.FilePath).HasMaxLength(500).IsRequired();
                e.Property(f => f.FileSizeBytes).IsRequired();
                e.Property(f => f.FileType).HasMaxLength(10).IsRequired();
                e.Property(f => f.MimeType).HasMaxLength(100).IsRequired();
                e.Property(f => f.UploadedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(f => f.UserId);

                e.HasOne(f => f.User)
                    .WithMany(u => u.FileUploads)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // GENERATED CONTENT
            // ==================================================================
            builder.Entity<GeneratedContent>(e =>
            {
                e.HasKey(g => g.Id);

                e.Property(g => g.ContentType).HasMaxLength(20).IsRequired();
                e.Property(g => g.GeneratedEntityId).IsRequired();
                e.Property(g => g.TopicSpecified).HasMaxLength(200);
                e.Property(g => g.GenerationConfig);
                e.Property(g => g.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(g => g.FileUploadId);
                e.HasIndex(g => new { g.GeneratedEntityId, g.ContentType });

                e.HasOne(g => g.FileUpload)
                    .WithMany(f => f.GeneratedContents)
                    .HasForeignKey(g => g.FileUploadId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(g => g.Course)
                    .WithMany(c => c.GeneratedContents)
                    .HasForeignKey(g => g.CourseId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ==================================================================
            // STUDY SCHEDULE
            // ==================================================================
            builder.Entity<StudySchedule>(e =>
            {
                e.HasKey(s => s.Id);

                e.Property(s => s.Title).HasMaxLength(200).IsRequired();
                e.Property(s => s.StartDatetime).IsRequired();
                e.Property(s => s.EndDatetime).IsRequired();
                e.Property(s => s.IsCompleted).HasDefaultValue(false);
                e.Property(s => s.NotificationSent).HasDefaultValue(false);
                e.Property(s => s.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(s => s.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(s => new { s.UserId, s.StartDatetime });

                e.HasOne(s => s.User)
                    .WithMany(u => u.StudySchedules)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // SCHEDULE INTERVAL
            // ==================================================================
            builder.Entity<ScheduleInterval>(e =>
            {
                e.HasKey(i => i.Id);

                e.Property(i => i.IntervalType).HasMaxLength(15).IsRequired();
                e.Property(i => i.DurationMinutes).IsRequired();
                e.Property(i => i.OrderPosition).HasDefaultValue(0);

                e.HasIndex(i => i.ScheduleId);

                e.HasOne(i => i.Schedule)
                    .WithMany(s => s.ScheduleIntervals)
                    .HasForeignKey(i => i.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // SCHEDULE CONTENT
            // ==================================================================
            builder.Entity<ScheduleContent>(e =>
            {
                e.HasKey(sc => sc.Id);

                e.Property(sc => sc.ContentType).HasMaxLength(20).IsRequired();
                e.Property(sc => sc.ContentId).IsRequired();
                e.Property(sc => sc.EstimatedMinutes);
                e.Property(sc => sc.Completed).HasDefaultValue(false);

                e.HasIndex(sc => sc.ScheduleId);
                e.HasIndex(sc => new { sc.ContentType, sc.ContentId });

                e.HasOne(sc => sc.Schedule)
                    .WithMany(s => s.ScheduleContents)
                    .HasForeignKey(sc => sc.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // NOTIFICATION
            // ==================================================================
            builder.Entity<Notification>(e =>
            {
                e.HasKey(n => n.Id);

                e.Property(n => n.Type).HasMaxLength(30).IsRequired();
                e.Property(n => n.Title).HasMaxLength(200).IsRequired();
                e.Property(n => n.Message).IsRequired();
                e.Property(n => n.RelatedEntityType).HasMaxLength(50);
                e.Property(n => n.IsRead).HasDefaultValue(false);
                e.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(n => new { n.UserId, n.IsRead, n.ScheduledFor });

                e.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================================
            // SYSTEM CONFIGURATION
            // ==================================================================
            builder.Entity<SystemConfiguration>(e =>
            {
                e.HasKey(sc => sc.Id);

                e.Property(sc => sc.ConfigKey).HasMaxLength(100).IsRequired();
                e.HasIndex(sc => sc.ConfigKey).IsUnique();
                e.Property(sc => sc.ConfigValue).IsRequired();
                e.Property(sc => sc.UpdatedAt).HasDefaultValueSql("GETDATE()");

                e.HasOne(sc => sc.UpdatedByUser)
                    .WithMany(u => u.SystemConfigurations)
                    .HasForeignKey(sc => sc.UpdatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==================================================================
            // ACTIVITY LOG
            // ==================================================================
            builder.Entity<ActivityLog>(e =>
            {
                e.HasKey(al => al.Id);

                e.Property(al => al.Action).HasMaxLength(100).IsRequired();
                e.Property(al => al.EntityType).HasMaxLength(50);
                e.Property(al => al.IpAddress).HasMaxLength(45);
                e.Property(al => al.CreatedAt).HasDefaultValueSql("GETDATE()");

                e.HasIndex(al => new { al.UserId, al.CreatedAt });
                e.HasIndex(al => new { al.EntityType, al.EntityId });

                e.HasOne(al => al.User)
                    .WithMany(u => u.ActivityLogs)
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
