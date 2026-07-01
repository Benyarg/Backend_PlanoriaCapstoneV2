using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Users.Requests;
using PlanoriaCapstone.DTOs.Users.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IWebHostEnvironment _environment;

    public UserService(IUserRepository userRepository, IActivityLogRepository activityLogRepository, IWebHostEnvironment environment)
    {
        _userRepository = userRepository;
        _activityLogRepository = activityLogRepository;
        _environment = environment;
    }

    public async Task<UserResponseDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        return MapToUserResponse(user);
    }

    public async Task<UserResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrWhiteSpace(request.Timezone))
            user.Timezone = request.Timezone;

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "UpdateProfile",
            EntityType = "User",
            EntityId = userId,
            Details = "Perfil actualizado",
            CreatedAt = DateTime.UtcNow
        });

        return MapToUserResponse(user);
    }

    public async Task UploadAvatarAsync(int userId, Stream avatarStream, string fileName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        var avatarsDir = Path.Combine(_environment.WebRootPath, "avatars");
        Directory.CreateDirectory(avatarsDir);

        var ext = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(avatarsDir, uniqueFileName);

        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            await avatarStream.CopyToAsync(fs);
        }

        user.Avatar = Path.Combine("avatars", uniqueFileName);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "UploadAvatar",
            EntityType = "User",
            EntityId = userId,
            Details = $"Avatar subido: {fileName}",
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task DeleteAvatarAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        if (!string.IsNullOrEmpty(user.Avatar))
        {
            var fullPath = Path.Combine(_environment.WebRootPath, user.Avatar);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        user.Avatar = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "DeleteAvatar",
            EntityType = "User",
            EntityId = userId,
            Details = "Avatar eliminado",
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<UserPreferencesResponseDto> GetPreferencesAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        return MapToPreferencesResponse(user);
    }

    public async Task<UserPreferencesResponseDto> UpdatePreferencesAsync(int userId, UpdatePreferencesRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(request.Theme))
            user.Theme = request.Theme;

        if (!string.IsNullOrWhiteSpace(request.PreferredLanguage))
            user.PreferredLanguage = request.PreferredLanguage;

        if (request.NotificationEnabled.HasValue)
            user.NotificationEnabled = request.NotificationEnabled.Value;

        if (request.EmailNotifications.HasValue)
            user.EmailNotifications = request.EmailNotifications.Value;

        if (request.DefaultSpacedRepetitionDays is { Count: > 0 })
            user.DefaultSpacedRepetitionDays = JsonSerializer.Serialize(request.DefaultSpacedRepetitionDays);

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "UpdatePreferences",
            EntityType = "User",
            EntityId = userId,
            Details = "Preferencias actualizadas",
            CreatedAt = DateTime.UtcNow
        });

        return MapToPreferencesResponse(user);
    }

    public async Task ResetDefaultsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        user.Theme = "light";
        user.PreferredLanguage = "en";
        user.NotificationEnabled = true;
        user.EmailNotifications = true;
        user.DefaultSpacedRepetitionDays = "[1,3,7,14,30]";
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "ResetDefaults",
            EntityType = "User",
            EntityId = userId,
            Details = "Preferencias restablecidas a valores por defecto",
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<NotificationSettingsResponseDto> GetNotificationSettingsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        return new NotificationSettingsResponseDto
        {
            StudyReminders = false,
            ExamAlerts = false,
            AchievementAlerts = false,
            ReminderTime = "08:00",
            ReminderDaysBeforeExam = 3
        };
    }

    public async Task<NotificationSettingsResponseDto> UpdateNotificationSettingsAsync(int userId, UpdateNotificationSettingsRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "UpdateNotificationSettings",
            EntityType = "User",
            EntityId = userId,
            Details = "Configuración de notificaciones actualizada",
            CreatedAt = DateTime.UtcNow
        });

        return new NotificationSettingsResponseDto
        {
            StudyReminders = request.StudyReminders ?? false,
            ExamAlerts = request.ExamAlerts ?? false,
            AchievementAlerts = request.AchievementAlerts ?? false,
            ReminderTime = request.ReminderTime ?? "08:00",
            ReminderDaysBeforeExam = request.ReminderDaysBeforeExam ?? 3
        };
    }

    public async Task TestNotificationAsync(int userId)
    {
        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "TestNotification",
            EntityType = "User",
            EntityId = userId,
            Details = "Notificación de prueba enviada",
            CreatedAt = DateTime.UtcNow
        });

        await Task.CompletedTask;
    }

    public async Task DeleteAccountAsync(int userId, DeleteAccountRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Contraseña incorrecta");

        if (request.ConfirmationText != "ELIMINAR")
            throw new InvalidOperationException("Texto de confirmación incorrecto");

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "DeleteAccount",
            EntityType = "User",
            EntityId = userId,
            Details = "Cuenta eliminada",
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<ExportDataResponseDto> ExportDataAsync(int userId, ExportDataRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "ExportData",
            EntityType = "User",
            EntityId = userId,
            Details = $"Datos exportados en formato {request.Format}",
            CreatedAt = DateTime.UtcNow
        });

        return new ExportDataResponseDto
        {
            DownloadUrl = string.Empty,
            FileSize = 0,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Formats = new List<string> { request.Format }
        };
    }

    public async Task DeactivateAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "Deactivate",
            EntityType = "User",
            EntityId = userId,
            Details = "Cuenta desactivada",
            CreatedAt = DateTime.UtcNow
        });
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

    private static UserPreferencesResponseDto MapToPreferencesResponse(User user)
    {
        List<int> spacedDays;
        try
        {
            spacedDays = JsonSerializer.Deserialize<List<int>>(user.DefaultSpacedRepetitionDays) ?? new List<int> { 1, 3, 7, 14, 30 };
        }
        catch
        {
            spacedDays = new List<int> { 1, 3, 7, 14, 30 };
        }

        return new UserPreferencesResponseDto
        {
            Theme = user.Theme,
            PreferredLanguage = user.PreferredLanguage,
            NotificationEnabled = user.NotificationEnabled,
            EmailNotifications = user.EmailNotifications,
            DefaultSpacedRepetitionDays = spacedDays,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
