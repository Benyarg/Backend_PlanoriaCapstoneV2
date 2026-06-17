using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Quiz.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Services
{
    [TestClass]
    public class QuizServiceTests
    {
        private AppDbContext _context = null!;
        private IQuizRepository _quizRepo = null!;
        private ICourseRepository _courseRepo = null!;
        private IActivityLogRepository _logRepo = null!;
        private QuizService _quizService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _quizRepo = new QuizRepository(_context);
            _courseRepo = new CourseRepository(_context);
            _logRepo = new ActivityLogRepository(_context);
            _quizService = new QuizService(_quizRepo, _courseRepo, _logRepo);

            _context.Courses.Add(new Course
            {
                Id = 1,
                Name = "Test Course",
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task CreateAsync_ValidQuiz_ReturnsQuizResponse()
        {
            var request = new CreateQuizRequestDto
            {
                Title = "Test Quiz",
                Description = "Quiz de prueba",
                CourseId = 1,
                PassingScore = 70,
                ShuffleQuestions = true,
                ShuffleOptions = true,
                AttemptsAllowed = 3
            };

            var result = await _quizService.CreateAsync(1, request);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Quiz", result.Title);
            Assert.AreEqual(1, result.CourseId);
            Assert.AreEqual(70, result.PassingScore);
            Assert.IsTrue(result.Id > 0);
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingQuiz_ReturnsQuiz()
        {
            var created = await _quizService.CreateAsync(1, new CreateQuizRequestDto
            {
                Title = "Get Test",
                CourseId = 1,
                PassingScore = 80
            });

            var result = await _quizService.GetByIdAsync(created.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Get Test", result.Title);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistent_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
                () => _quizService.GetByIdAsync(999));
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingQuiz_ReturnsTrue()
        {
            var created = await _quizService.CreateAsync(1, new CreateQuizRequestDto
            {
                Title = "Delete Test",
                CourseId = 1
            });

            var result = await _quizService.DeleteAsync(created.Id);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CreateQuestionAsync_ValidQuestion_AddsToQuiz()
        {
            var quiz = await _quizService.CreateAsync(1, new CreateQuizRequestDto
            {
                Title = "Questions Quiz",
                CourseId = 1
            });

            var request = new CreateQuestionRequestDto
            {
                QuestionText = "¿Cuánto es 2+2?",
                QuestionType = "multiple_choice",
                Explanation = "Suma básica",
                Points = 1,
                OrderPosition = 1,
                Options = new List<CreateOptionRequestDto>
                {
                    new() { OptionText = "3", IsCorrect = false, OrderPosition = 1 },
                    new() { OptionText = "4", IsCorrect = true, OrderPosition = 2 },
                    new() { OptionText = "5", IsCorrect = false, OrderPosition = 3 },
                    new() { OptionText = "6", IsCorrect = false, OrderPosition = 4 }
                }
            };

            var result = await _quizService.CreateQuestionAsync(quiz.Id, request);

            Assert.IsNotNull(result);
            Assert.AreEqual("¿Cuánto es 2+2?", result.QuestionText);
            Assert.AreEqual(4, result.Options.Count);
        }
    }
}