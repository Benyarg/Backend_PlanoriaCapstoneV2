using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Auth.Requests;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private AppDbContext _context = null!;
        private IUserRepository _userRepo = null!;
        private IActivityLogRepository _logRepo = null!;
        private IConfiguration _config = null!;
        private AuthService _authService = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _userRepo = new UserRepository(_context);
            _logRepo = new ActivityLogRepository(_context);

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "EstaEsUnaClaveSuperSecretaDe32Caracteres!" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpireMinutes", "60" }
                });

            _config = configBuilder.Build();
            _authService = new AuthService(_userRepo, _logRepo, _config);
        }

        [TestMethod]
        public async Task RegisterAsync_ValidUser_ReturnsAuthResponse()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Nombre = "Test",
                Apellido = "User",
                Email = "test@test.com",
                Password = "Password123!",
                PreferredLanguage = "es"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AccessToken);
            Assert.IsNotNull(result.RefreshToken);
            Assert.AreEqual("Bearer", result.TokenType);
            Assert.IsNotNull(result.User);
            Assert.AreEqual("test@test.com", result.User.Email);
        }

        [TestMethod]
        public async Task RegisterAsync_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Nombre = "Test",
                Apellido = "User",
                Email = "duplicate@test.com",
                Password = "Password123!"
            };

            await _authService.RegisterAsync(request);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request));
        }

        [TestMethod]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            await _authService.RegisterAsync(new RegisterRequestDto
            {
                Nombre = "Login",
                Apellido = "Test",
                Email = "login@test.com",
                Password = "Password123!"
            });

            var loginRequest = new LoginRequestDto
            {
                Email = "login@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AccessToken);
        }

        [TestMethod]
        public async Task LoginAsync_InvalidPassword_ThrowsException()
        {
            // Arrange
            await _authService.RegisterAsync(new RegisterRequestDto
            {
                Nombre = "Bad",
                Apellido = "Login",
                Email = "bad@test.com",
                Password = "Password123!"
            });

            var loginRequest = new LoginRequestDto
            {
                Email = "bad@test.com",
                Password = "WrongPassword!"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
    () => _authService.LoginAsync(loginRequest));
        }
    }
}