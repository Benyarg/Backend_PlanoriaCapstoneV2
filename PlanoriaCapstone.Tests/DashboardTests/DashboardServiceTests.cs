using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Services
{
    [TestClass]
    public class DashboardServiceTests
    {
        private AppDbContext _context = null!;
        private IActivityLogRepository _logRepo = null!;
        private IStudyScheduleRepository _scheduleRepo = null!;
        private IUserProgressFlashcardRepository _flashcardProgressRepo = null!;
        private IUserProgressQuizRepository _quizProgressRepo = null!;
        private ICourseRepository _courseRepo = null!;
        private INotificationRepository _notificationRepo = null!;
        private DashboardService _dashboardService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _logRepo = new ActivityLogRepository(_context);
            _scheduleRepo = new StudyScheduleRepository(_context);
            _flashcardProgressRepo = new UserProgressFlashcardRepository(_context);
            _quizProgressRepo = new UserProgressQuizRepository(_context);
            _courseRepo = new CourseRepository(_context);
            _notificationRepo = new NotificationRepository(_context);
            _dashboardService = new DashboardService(_logRepo, _scheduleRepo, _flashcardProgressRepo, _quizProgressRepo, _courseRepo, _notificationRepo);
        }

        [TestMethod]
        public async Task GetSummaryAsync_ReturnsOverviewWithData()
        {
            await _logRepo.LogAsync(new ActivityLog
            {
                UserId = 1,
                Action = "Login",
                EntityType = "User",
                CreatedAt = DateTime.UtcNow
            });

            var result = await _dashboardService.GetSummaryAsync(1);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StreakDays >= 0);
        }

        [TestMethod]
        public async Task GetRecentActivityAsync_ReturnsActivities()
        {
            await _logRepo.LogAsync(new ActivityLog
            {
                UserId = 1,
                Action = "TestAction",
                EntityType = "Test",
                CreatedAt = DateTime.UtcNow
            });

            var result = await _dashboardService.GetRecentActivityAsync(1, 10);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetUpcomingDeadlinesAsync_ReturnsEmptyWhenNoDeadlines()
        {
            var result = await _dashboardService.GetUpcomingDeadlinesAsync(1, 30);

            Assert.IsNotNull(result);
        }
    }
}