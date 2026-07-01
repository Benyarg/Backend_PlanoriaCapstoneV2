using Microsoft.AspNetCore.Hosting;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Dal;
using PlanoriaCapstone.DTOs.Files.Responses;
using PlanoriaCapstone.Models;

namespace PlanoriaCapstone.Bll.Service;

public class FileService : IFileService
{
    private readonly IFileUploadRepository _fileRepository;
    private readonly IWebHostEnvironment _env;
    private readonly IActivityLogRepository _activityLogRepository;

    public FileService(
        IFileUploadRepository fileRepository,
        IWebHostEnvironment env,
        IActivityLogRepository activityLogRepository)
    {
        _fileRepository = fileRepository;
        _env = env;
        _activityLogRepository = activityLogRepository;
    }


    //region CRUD - Gestión de Archivos
    public async Task<FileUploadResponseDto> UploadAsync(int userId, int courseId, Stream fileStream, string fileName, string contentType, long fileSize)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "assets", "uploads", userId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var uniqueFileName = $"{Guid.NewGuid():N}_{fileName}";
        var filePath = Path.Combine(uploadsDir, uniqueFileName);

        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        var ext = Path.GetExtension(fileName)?.TrimStart('.').ToLower() ?? "unknown";

        var fileUpload = new FileUpload
        {
            UserId = userId,
            OriginalFilename = fileName,
            FilePath = Path.Combine("assets", "uploads", userId.ToString(), uniqueFileName),
            FileSizeBytes = fileSize,
            FileType = ext,
            MimeType = contentType,
            UploadedAt = DateTime.UtcNow
        };

        var created = await _fileRepository.CreateAsync(fileUpload);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = userId,
            Action = "file.uploaded",
            EntityType = "FileUpload",
            EntityId = created.Id,
            Details = $"Uploaded file '{fileName}' ({fileSize} bytes)",
            CreatedAt = DateTime.UtcNow
        });

        return MapToUploadResponseDto(created);
    }

    public async Task<FileUploadResponseDto?> GetUploadStatusAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        return file == null ? null : MapToUploadResponseDto(file);
    }

    public async Task<IEnumerable<FileUploadResponseDto>> GetUploadHistoryAsync(int userId)
    {
        var files = await _fileRepository.GetByUserIdAsync(userId);
        return files.Select(MapToUploadResponseDto);
    }

    public async Task<bool> DeleteUploadAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null) return false;

        var fullPath = Path.Combine(_env.WebRootPath, file.FilePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return await _fileRepository.DeleteAsync(fileId);
    }

    //Processing - Lógica de Conversión e IA
    public async Task<FileProcessingStatusResponseDto> ProcessFileAsync(int fileId, int targetCourseId, string contentType)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            return new FileProcessingStatusResponseDto
            {
                FileId = fileId,
                Status = "failed",
                ProgressPercentage = 0,
                EstimatedTimeRemaining = 0
            };
        }

        var generated = new GeneratedContent
        {
            FileUploadId = fileId,
            CourseId = targetCourseId,
            ContentType = contentType,
            GeneratedEntityId = 0,
            GenerationConfig = "{}",
            CreatedAt = DateTime.UtcNow
        };

        await _fileRepository.CreateGeneratedContentAsync(generated);

        file.ProcessedAt = DateTime.UtcNow;
        await _fileRepository.UpdateAsync(file);

        await _activityLogRepository.LogAsync(new ActivityLog
        {
            UserId = file.UserId,
            Action = "file.processed",
            EntityType = "FileUpload",
            EntityId = fileId,
            Details = $"Processed file '{file.OriginalFilename}' as {contentType}",
            CreatedAt = DateTime.UtcNow
        });

        return new FileProcessingStatusResponseDto
        {
            FileId = fileId,
            Status = "completed",
            ProgressPercentage = 100,
            EstimatedTimeRemaining = 0
        };
    }

    public async Task<FileProcessingStatusResponseDto?> GetProcessingStatusAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null) return null;

        var status = file.ProcessedAt.HasValue ? "completed" : "pending";

        return new FileProcessingStatusResponseDto
        {
            FileId = fileId,
            Status = status,
            ProgressPercentage = file.ProcessedAt.HasValue ? 100 : 0,
            EstimatedTimeRemaining = 0
        };
    }

    public async Task<FileProcessingStatusResponseDto> ReprocessAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            return new FileProcessingStatusResponseDto
            {
                FileId = fileId,
                Status = "failed",
                ProgressPercentage = 0,
                EstimatedTimeRemaining = 0
            };
        }

        var existingContent = await _fileRepository.GetGeneratedContentAsync(fileId);
        if (existingContent != null)
        {
            var courseId = existingContent.CourseId;
            return await ProcessFileAsync(fileId, courseId, existingContent.ContentType);
        }

        file.ProcessedAt = null;
        await _fileRepository.UpdateAsync(file);

        return new FileProcessingStatusResponseDto
        {
            FileId = fileId,
            Status = "pending",
            ProgressPercentage = 0,
            EstimatedTimeRemaining = 0
        };
    }

    //Access - Descarga y Streams
    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId)
            ?? throw new FileNotFoundException($"File with ID {fileId} not found.");

        var fullPath = Path.Combine(_env.WebRootPath, file.FilePath);
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        return (stream, file.MimeType, file.OriginalFilename);
    }

    //NOTE: no tine endpoint en el controlador
    public async Task<string?> GetFileUrlAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        return file == null ? null : $"/{file.FilePath.Replace("\\", "/")}";
    }

    public async Task<Stream> StreamFileAsync(int fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId)
            ?? throw new FileNotFoundException($"File with ID {fileId} not found.");

        var fullPath = Path.Combine(_env.WebRootPath, file.FilePath);
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
    }

    //Private Helpers
    private static FileUploadResponseDto MapToUploadResponseDto(FileUpload file)
    {
        return new FileUploadResponseDto
        {
            Id = file.Id,
            UserId = file.UserId,
            OriginalFilename = file.OriginalFilename,
            FileSize = file.FileSizeBytes,
            FileType = file.FileType,
            UploadStatus = file.ProcessedAt.HasValue ? "processed" : "uploaded",
            UploadedAt = file.UploadedAt,
            ProcessedAt = file.ProcessedAt
        };
    }
}
