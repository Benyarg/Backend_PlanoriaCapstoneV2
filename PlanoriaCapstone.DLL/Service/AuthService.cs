using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Auth.Requests;
using PlanoriaCapstone.DTOs.Auth.Responses;
using PlanoriaCapstone.DTOs.Users.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IActivityLogRepository activityLogRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _activityLogRepository = activityLogRepository;
        _configuration = configuration;
    }

    //OPERACIONES PRINCIPALES
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("El email ya está registrado");

        var user = new User
        {
            FullName = $"{request.Nombre} {request.Apellido}".Trim(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PreferredLanguage = request.PreferredLanguage,
            Theme = "light",
            Timezone = "UTC",
            NotificationEnabled = true,
            EmailNotifications = true,
            DefaultSpacedRepetitionDays = "[1,3,7,14,30]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = created.Id,
            Action = "Register",
            EntityType = "User",
            EntityId = created.Id,
            Details = "Usuario registrado",
            CreatedAt = DateTime.UtcNow
        });

        return await GenerateAuthResponse(created);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || user.DeletedAt != null)
            throw new InvalidOperationException("Credenciales inválidas");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Credenciales inválidas");

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = user.Id,
            Action = "Login",
            EntityType = "User",
            EntityId = user.Id,
            Details = "Inicio de sesión",
            CreatedAt = DateTime.UtcNow
        });

        return await GenerateAuthResponse(user);
    }

    public async Task LogoutAsync(int userId)
    {
        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "Logout",
            EntityType = "User",
            EntityId = userId,
            Details = "Cierre de sesión",
            CreatedAt = DateTime.UtcNow
        });

        await Task.CompletedTask;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        await Task.CompletedTask;
        return null!;
    }

    public async Task VerifyEmailAsync(VerifyEmailRequestDto request)
    {
        await Task.CompletedTask;
    }

    public async Task<VerificationSentResponseDto> ResendVerificationAsync(ResendVerificationRequestDto request)
    {
        return await Task.FromResult(new VerificationSentResponseDto
        {
            Message = "Si el email existe, recibirás un enlace de verificación",
            Email = request.Email,
            ResentAt = DateTime.UtcNow
        });
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        await Task.CompletedTask;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        await Task.CompletedTask;
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("La contraseña actual es incorrecta");

        if (request.NewPassword != request.NewPasswordConfirmation)
            throw new InvalidOperationException("Las nuevas contraseñas no coinciden");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "ChangePassword",
            EntityType = "User",
            EntityId = userId,
            Details = "Contraseña cambiada",
            CreatedAt = DateTime.UtcNow
        });
    }

    //UTILIDADES INTERNAS
    private async Task<AuthResponseDto> GenerateAuthResponse(User user)
    {
        var token = GenerateJwtToken(user);
        var expiresIn = _configuration.GetValue<int>("Jwt:ExpireMinutes", 60);

        var response = new AuthResponseDto
        {
            AccessToken = token,
            RefreshToken = string.Empty,
            TokenType = "Bearer",
            ExpiresIn = expiresIn * 60,
            User = MapToUserResponse(user)
        };

        return await Task.FromResult(response);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var expiresIn = _configuration.GetValue<int>("Jwt:ExpireMinutes", 60);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponseDto MapToUserResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Avatar = user.Avatar ?? string.Empty,
            Timezone = user.Timezone,
            PreferredLanguage = user.PreferredLanguage,
            Theme = user.Theme,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
