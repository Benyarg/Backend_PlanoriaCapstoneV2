//Revisado
using Microsoft.AspNetCore.Mvc;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.DTOs.Files.Requests;

namespace Backend_PlanoriaCapstone.Controllers
{
    [Route("api/files")]
    public class FilesController : BaseController
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        public class FileUploadRequest
        {
            public int CourseId { get; set; }
            public required IFormFile File { get; set; }
        }


        //region CRUD - Gestión de Archivos

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadRequest request)
        {
            var userId = GetUserId();
            using var stream = request.File.OpenReadStream();
            var result = await _fileService.UploadAsync(userId, request.CourseId, stream, request.File.FileName, request.File.ContentType, request.File.Length);
            return Ok(result);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetUploadStatus(int id)
        {
            var userId = GetUserId();
            var result = await _fileService.GetUploadStatusAsync(id);
            if (result == null) return NotFound();
            if (result.UserId != userId)
                return Forbidden();

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetUploadHistory()
        {
            var userId = GetUserId();
            var history = await _fileService.GetUploadHistoryAsync(userId);
            return Ok(history);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUpload(int id)
        {
            var userId = GetUserId();
            var file = await _fileService.GetUploadStatusAsync(id);
            if (file == null) return NotFound();
            if (file.UserId != userId)
                return Forbidden();

            var deleted = await _fileService.DeleteUploadAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        //Processing - Lógica de Conversión e IA
        [HttpPost("{id}/process")]
        public async Task<IActionResult> ProcessFile(int id, [FromBody] ProcessFileRequestDto request)
        {
            var userId = GetUserId();
            var file = await _fileService.GetUploadStatusAsync(id);
            if (file == null) return NotFound();
            if (file.UserId != userId)
                return Forbidden();

            var result = await _fileService.ProcessFileAsync(id, request.TargetCourseId, request.ContentFormat);
            return Ok(result);
        }

        [HttpGet("{id}/processing-status")]
        public async Task<IActionResult> GetProcessingStatus(int id)
        {
            var userId = GetUserId();
            var file = await _fileService.GetUploadStatusAsync(id);
            if (file == null) return NotFound();
            if (file.UserId != userId)
                return Forbidden();

            var result = await _fileService.GetProcessingStatusAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost("{id}/reprocess")]
        public async Task<IActionResult> Reprocess(int id)
        {
            var userId = GetUserId();
            var file = await _fileService.GetUploadStatusAsync(id);
            if (file == null) return NotFound();
            if (file.UserId != userId)
                return Forbidden();

            var result = await _fileService.ReprocessAsync(id);
            return Ok(result);
        }

        //Access - Descarga y Streams
        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            var userId = GetUserId();
            var file = await _fileService.GetUploadStatusAsync(id);
            if (file == null) return NotFound();
            if (file.UserId != userId)
                return Forbidden();

            try
            {
                var (stream, contentType, fileName) = await _fileService.DownloadAsync(id);
                return File(stream, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/stream")]
        public async Task<IActionResult> StreamFile(int id)
        {
            var userId = GetUserId();
            var file = await _fileService.GetUploadStatusAsync(id);
            if (file == null) return NotFound();
            if (file.UserId != userId)
                return Forbidden();

            try
            {
                var stream = await _fileService.StreamFileAsync(id);
                return File(stream, "application/octet-stream");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
