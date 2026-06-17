using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Quiz.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Services
{
    [TestClass]
    public class QuizAttemptServiceTests
    {
        private AppDbContext _context = null!;
        private IQuizAttemptRepository _attemptRepo = null!;
        private IQuizRepository _quizRepo = null!;
        private IUserProgressQuizRepository _progressRepo = null!;
        private IActivityLogRepository _logRepo = null!;
        private IQuizService _quizService = null!;
        private ICourseRepository _courseRepo = null!;
        private QuizAttemptService _attemptService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _attemptRepo = new QuizAttemptRepository(_context);
            _quizRepo = new QuizRepository(_context);
            _progressRepo = new UserProgressQuizRepository(_context);
            _logRepo = new ActivityLogRepository(_context);
            _courseRepo = new CourseRepository(_context);
            _quizService = new QuizService(_quizRepo, _courseRepo, _logRepo);
            _attemptService = new QuizAttemptService(_attemptRepo, _quizRepo, _progressRepo, _logRepo);

            _context.Courses.Add(new Course { Id = 1, Name = "Test", UserId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            _context.SaveChanges();
        }

        private async Task<(int quizId, List<int> questionIds, List<int> correctOptionIds)> CreateQuizWithQuestions()
        {
            var quiz = await _quizService.CreateAsync(1, new CreateQuizRequestDto
            {
                Title = "Grade Test",
                CourseId = 1,
                PassingScore = 70
            });

            var questions = new List<int>();
            var correctOptions = new List<int>();

            for (int i = 1; i <= 3; i++)
            {
                var q = await _quizService.CreateQuestionAsync(quiz.Id, new CreateQuestionRequestDto
                {
                    QuestionText = $"Pregunta {i}",
                    QuestionType = "multiple_choice",
                    Points = 1,
                    OrderPosition = i,
                    Options = new List<CreateOptionRequestDto>
                    {
                        new() { OptionText = "Incorrecta", IsCorrect = false, OrderPosition = 1 },
                        new() { OptionText = $"Correcta {i}", IsCorrect = true, OrderPosition = 2 },
                        new() { OptionText = "Incorrecta", IsCorrect = false, OrderPosition = 3 },
                        new() { OptionText = "Incorrecta", IsCorrect = false, OrderPosition = 4 }
                    }
                });
                questions.Add(q.Id);
                correctOptions.Add(q.Options[1].Id);
            }

            return (quiz.Id, questions, correctOptions);
        }

        [TestMethod]
        public async Task StartAsync_ValidQuiz_CreatesAttempt()
        {
            var (quizId, _, _) = await CreateQuizWithQuestions();

            var result = await _attemptService.StartAsync(1, new StartQuizAttemptRequestDto { QuizId = quizId });

            Assert.IsNotNull(result);
            Assert.AreEqual(quizId, result.QuizId);
            Assert.AreEqual(0, result.AnswersCount);
        }

        [TestMethod]
        public async Task SubmitAsync_AllCorrect_PassesQuiz()
        {
            var (quizId, questionIds, correctOptionIds) = await CreateQuizWithQuestions();
            var attempt = await _attemptService.StartAsync(1, new StartQuizAttemptRequestDto { QuizId = quizId });

            var answers = questionIds.Select((q, i) => new SubmitAnswerRequestDto
            {
                AttemptId = attempt.Id,
                QuestionId = q,
                SelectedOptionId = correctOptionIds[i],
                ShortAnswerText = ""
            }).ToList();

            var result = await _attemptService.SubmitAsync(1, new SubmitQuizRequestDto
            {
                AttemptId = attempt.Id,
                Answers = answers
            });

            Assert.IsNotNull(result);
            Assert.AreEqual(100, result.Attempt.ScorePercentage);
            Assert.IsTrue(result.Attempt.Passed == true);
            Assert.AreEqual(3, result.Attempt.CorrectAnswersCount);
        }

        [TestMethod]
        public async Task SubmitAsync_AllWrong_FailsQuiz()
        {
            var (quizId, questionIds, _) = await CreateQuizWithQuestions();
            var attempt = await _attemptService.StartAsync(1, new StartQuizAttemptRequestDto { QuizId = quizId });

            var answers = questionIds.Select(q => new SubmitAnswerRequestDto
            {
                AttemptId = attempt.Id,
                QuestionId = q,
                SelectedOptionId = 1,
                ShortAnswerText = ""
            }).ToList();

            var result = await _attemptService.SubmitAsync(1, new SubmitQuizRequestDto
            {
                AttemptId = attempt.Id,
                Answers = answers
            });

            Assert.AreEqual(0, result.Attempt.ScorePercentage);
            Assert.IsFalse(result.Attempt.Passed ?? true);
            Assert.AreEqual(0, result.Attempt.CorrectAnswersCount);
        }
    }
}