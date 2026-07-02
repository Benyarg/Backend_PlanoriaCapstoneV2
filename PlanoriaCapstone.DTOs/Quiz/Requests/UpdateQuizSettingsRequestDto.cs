namespace PlanoriaCapstone.DTOs.Quiz.Requests
{
    public class UpdateQuizSettingsRequestDto
    {
        public bool ShowResults { get; set; }
        public bool AllowRetries { get; set; }
        public int MaxAttempts { get; set; }
        public int TimePerQuestion { get; set; }
    }
}
