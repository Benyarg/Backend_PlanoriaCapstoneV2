using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanoriaCapstone.DTOs.Dashboard.Requests
{
    public class ExportDashboardRequestDto
    {
        [Required(ErrorMessage = "El formato es obligatorio.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "El formato debe tener entre {2} y {1} caracteres.")]
        public string Format { get; set; }

        public bool IncludeCharts { get; set; }
        public bool IncludeRawData { get; set; }
        public DateRange DateRange { get; set; }
        public List<int> CourseIds { get; set; }
    }
}
