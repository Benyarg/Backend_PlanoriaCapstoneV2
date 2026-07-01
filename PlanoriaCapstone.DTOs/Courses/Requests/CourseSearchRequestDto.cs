using System.ComponentModel.DataAnnotations;

namespace PlanoriaCapstone.DTOs.Courses.Requests
{
    public class CourseSearchRequestDto
    {
        [StringLength(200, ErrorMessage = "La consulta no puede exceder 200 caracteres")]
        public string? Query { get; set; } // El '?' lo hace opcional

        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string? Status { get; set; }

        [StringLength(20, ErrorMessage = "El campo de ordenamiento no puede exceder 20 caracteres")]
        public string? SortBy { get; set; }

        [StringLength(4, ErrorMessage = "El sentido de ordenamiento no puede exceder 4 caracteres")]
        public string? SortOrder { get; set; }

        [Range(1, 100, ErrorMessage = "Elementos por página debe estar entre 1 y 100")]
        public int PerPage { get; set; } = 10;

        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor o igual a 1")]
        public int Page { get; set; } = 1;
    }
}
