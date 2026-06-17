using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Flashcards.Cards.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Services
{
    [TestClass]
    public class FlashcardServiceTests
    {
        private AppDbContext _context = null!;
        private IFlashcardRepository _flashcardRepo = null!;
        private IFlashcardDeckRepository _deckRepo = null!;
        private IActivityLogRepository _logRepo = null!;
        private FlashcardService _flashcardService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _flashcardRepo = new FlashcardRepository(_context);
            _deckRepo = new FlashcardDeckRepository(_context);
            _logRepo = new ActivityLogRepository(_context);
            _flashcardService = new FlashcardService(_flashcardRepo, _deckRepo, _logRepo);

            // Seed deck
            _context.FlashcardDecks.Add(new FlashcardDeck
            {
                Id = 1,
                Name = "Test Deck",
                CourseId = 1,
                TotalCards = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task CreateAsync_ValidFlashcard_ReturnsFlashcard()
        {
            var request = new CreateFlashcardRequestDto
            {
                Question = "¿Qué es MSTest?",
                Answer = "Framework de pruebas unitarias de Microsoft",
                Difficulty = "easy",
                DeckId = 1,
                Position = 1,
                Tags = new List<string> { "testing", "csharp" }
            };

            var result = await _flashcardService.CreateAsync(request);

            Assert.IsNotNull(result);
            Assert.AreEqual("¿Qué es MSTest?", result.Question);
            Assert.AreEqual("easy", result.Difficulty);
            Assert.AreEqual(1, result.DeckId);
        }

        [TestMethod]
        public async Task GetByDeckIdAsync_ReturnsCards()
        {
            await _flashcardService.CreateAsync(new CreateFlashcardRequestDto
            {
                Question = "Q1",
                Answer = "A1",
                DeckId = 1,
                Position = 1
            });
            await _flashcardService.CreateAsync(new CreateFlashcardRequestDto
            {
                Question = "Q2",
                Answer = "A2",
                DeckId = 1,
                Position = 2
            });

            var result = await _flashcardService.GetByDeckIdAsync(1);

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_ExistingCard_UpdatesCorrectly()
        {
            var created = await _flashcardService.CreateAsync(new CreateFlashcardRequestDto
            {
                Question = "Original",
                Answer = "Original A",
                DeckId = 1,
                Position = 1
            });

            var update = new UpdateFlashcardRequestDto
            {
                Question = "Actualizada",
                Answer = "Respuesta actualizada",
                Difficulty = "hard"
            };

            var result = await _flashcardService.UpdateAsync(created.Id, update);

            Assert.AreEqual("Actualizada", result.Question);
            Assert.AreEqual("hard", result.Difficulty);
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingCard_ReturnsTrue()
        {
            var created = await _flashcardService.CreateAsync(new CreateFlashcardRequestDto
            {
                Question = "To Delete",
                Answer = "Delete Me",
                DeckId = 1,
                Position = 1
            });

            var result = await _flashcardService.DeleteAsync(created.Id);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task BulkCreateAsync_CreatesMultipleCards()
        {
            var request = new BulkCreateFlashcardsRequestDto
            {
                DeckId = 1,
                Cards = new List<CreateFlashcardRequestDto>
                {
                    new() { Question = "Q1", Answer = "A1", DeckId = 1, Position = 1 },
                    new() { Question = "Q2", Answer = "A2", DeckId = 1, Position = 2 },
                    new() { Question = "Q3", Answer = "A3", DeckId = 1, Position = 3 }
                }
            };

            var result = await _flashcardService.BulkCreateAsync(request);

            Assert.AreEqual(3, result.Count());
        }
    }
}