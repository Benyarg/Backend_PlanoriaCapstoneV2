using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.IA.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Services
{
    [TestClass]
    public class AiGenerationServiceTests
    {
        private AppDbContext _context = null!;
        private IFileUploadRepository _fileRepo = null!;
        private IActivityLogRepository _logRepo = null!;
        private IFlashcardDeckService _deckService = null!;
        private IFlashcardService _flashcardService = null!;
        private IQuizService _quizService = null!;
        private IConfiguration _config = null!;
        private AiGenerationService _aiService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _fileRepo = new FileUploadRepository(_context);
            _logRepo = new ActivityLogRepository(_context);

            var deckRepo = new FlashcardDeckRepository(_context);
            var flashcardRepo = new FlashcardRepository(_context);
            var progressRepo = new UserProgressFlashcardRepository(_context);
            _flashcardService = new FlashcardService(flashcardRepo, deckRepo, _logRepo);
            _deckService = new FlashcardDeckService(deckRepo, flashcardRepo, progressRepo, _logRepo);

            var quizRepo = new QuizRepository(_context);
            var courseRepo = new CourseRepository(_context);
            _quizService = new QuizService(quizRepo, courseRepo, _logRepo);

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "AI:Provider", "groq" },
                    { "AI:ApiKey", "" },
                    { "AI:Model", "llama-3.3-70b-versatile" }
                });
            _config = configBuilder.Build();

            _aiService = new AiGenerationService(_fileRepo, _logRepo, _deckService, _flashcardService, _quizService, _config);

            _context.FileUploads.Add(new FileUpload
            {
                Id = 1,
                UserId = 1,
                OriginalFilename = "test.pdf",
                FilePath = "test.pdf",
                FileType = "pdf",
                FileSizeBytes = 1000,
                UploadedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task GenerateFlashcardsAsync_NoApiKey_ThrowsException()
        {
            var request = new GenerateContentRequestDto
            {
                FileId = 1,
                TargetCourseId = 1,
                NumberOfItems = 3,
                Difficulty = "medium",
                Language = "es"
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _aiService.GenerateFlashcardsAsync(1, request));
        }

        [TestMethod]
        public async Task GenerateQuizAsync_NoApiKey_ThrowsException()
        {
            var request = new GenerateContentRequestDto
            {
                FileId = 1,
                TargetCourseId = 1,
                NumberOfItems = 3,
                Difficulty = "easy",
                Language = "es"
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _aiService.GenerateQuizAsync(1, request));
        }

        [TestMethod]
        public async Task GetGenerationStatusAsync_InvalidId_ThrowsException()
        {
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
                () => _aiService.GetGenerationStatusAsync(999));
        }

        [TestMethod]
        public async Task SetConfigAsync_ValidConfig_SetsConfiguration()
        {
            var config = new AIConfigRequestDto
            {
                Provider = "groq",
                ApiKey = "test-key-123",
                Model = "llama-3.3-70b-versatile",
                MaxTokens = 1000,
                Temperature = 0.5m
            };

            await _aiService.SetConfigAsync(config);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task GetConfigAsync_ReturnsConfiguration()
        {
            await _aiService.SetConfigAsync(new AIConfigRequestDto
            {
                Provider = "groq",
                ApiKey = "test-key",
                Model = "llama-3.3-70b-versatile"
            });

            var result = await _aiService.GetConfigAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("groq", result.Provider);
            Assert.IsTrue(result.IsActive);
        }
    }
}