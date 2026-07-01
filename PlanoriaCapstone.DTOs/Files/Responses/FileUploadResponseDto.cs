using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Files.Responses
{
    public class FileUploadResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OriginalFilename { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string UploadStatus { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}