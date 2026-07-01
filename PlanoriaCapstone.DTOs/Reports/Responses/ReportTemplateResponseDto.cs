
namespace PlanoriaCapstone.DTOs.Reports.Responses
{
    public class ReportTemplateResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Config { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
