namespace WebReckrytingSystem.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public string StudentEmail { get; set; } = string.Empty;
        public string VacancyCompanyName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public string? CoverLetter { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }
}