using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Courses.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Services
{
    [TestClass]
    public class CourseServiceTests
    {
        private AppDbContext _context = null!;
        private ICourseRepository _courseRepo = null!;
        private IUserCourseExamProgressRepository _examRepo = null!;
        private IUserRepository _userRepo = null!;
        private IActivityLogRepository _logRepo = null!;
        private CourseService _courseService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _courseRepo = new CourseRepository(_context);
            _examRepo = new UserCourseExamProgressRepository(_context);
            _userRepo = new UserRepository(_context);
            _logRepo = new ActivityLogRepository(_context);
            _courseService = new CourseService(_courseRepo, _examRepo, _userRepo, _logRepo);
        }

        [TestMethod]
        public async Task CreateAsync_ValidCourse_ReturnsCourse()
        {
            var request = new CreateCourseRequestDto
            {
                Name = "Matemáticas",
                Description = "Curso de álgebra",
                ColorHex = "#3498db"
            };

            var result = await _courseService.CreateAsync(1, request);

            Assert.IsNotNull(result);
            Assert.AreEqual("Matemáticas", result.Name);
            Assert.AreEqual("#3498db", result.ColorHex);
            Assert.IsFalse(result.IsArchived);
        }

        [TestMethod]
        public async Task GetByUserIdAsync_ReturnsUserCourses()
        {
            await _courseService.CreateAsync(1, new CreateCourseRequestDto { Name = "Curso 1" });
            await _courseService.CreateAsync(1, new CreateCourseRequestDto { Name = "Curso 2" });

            var result = await _courseService.GetByUserIdAsync(1);

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_UpdatesCourseName()
        {
            var created = await _courseService.CreateAsync(1, new CreateCourseRequestDto { Name = "Original" });
            var update = new UpdateCourseRequestDto { Name = "Actualizado" };

            var result = await _courseService.UpdateAsync(created.Id, update);

            Assert.AreEqual("Actualizado", result.Name);
        }

        [TestMethod]
        public async Task ArchiveAsync_ArchivesCourse()
        {
            var created = await _courseService.CreateAsync(1, new CreateCourseRequestDto { Name = "Archivable" });

            await _courseService.ArchiveAsync(created.Id);
            var result = await _courseService.GetByIdAsync(created.Id);

            Assert.IsTrue(result.IsArchived);
        }

        [TestMethod]
        public async Task RestoreAsync_RestoresArchivedCourse()
        {
            var created = await _courseService.CreateAsync(1, new CreateCourseRequestDto { Name = "Restaurable" });
            await _courseService.ArchiveAsync(created.Id);

            await _courseService.RestoreAsync(created.Id);
            var result = await _courseService.GetByIdAsync(created.Id);

            Assert.IsFalse(result.IsArchived);
        }

        [TestMethod]
        public async Task SetExamDateAsync_SetsDateCorrectly()
        {
            var created = await _courseService.CreateAsync(1, new CreateCourseRequestDto { Name = "Con Examen" });
            var examDate = new DateTime(2026, 12, 15);

            await _courseService.SetExamDateAsync(created.Id, new SetExamDateRequestDto
            {
                ExamDate = examDate,
                ExamTime = "10:00",
                NotifyMe = true
            });
            var result = await _courseService.GetByIdAsync(created.Id);

            Assert.AreEqual(examDate.Date, result.ExamDate?.Date);
        }
    }
}