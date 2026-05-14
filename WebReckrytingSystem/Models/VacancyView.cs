namespace WebReckrytingSystem.Models
{
    public class VacancyView
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string VacancyCompanyName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}