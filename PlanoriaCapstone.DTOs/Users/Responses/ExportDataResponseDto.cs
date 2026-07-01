using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Users.Responses
{
    public class ExportDataResponseDto
    {
        [Required(ErrorMessage = "La URL de descarga es requerida")]
        [Url(ErrorMessage = "Formato de URL inválido")]
        [StringLength(2048, ErrorMessage = "La URL de descarga no puede exceder los 2048 caracteres")]
        public string DownloadUrl { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = "El tamaño del archivo debe ser mayor o igual a 0")]
        public long FileSize { get; set; }

        public DateTime ExpiresAt { get; set; }

        public List<string> Formats { get; set; }
    }
}