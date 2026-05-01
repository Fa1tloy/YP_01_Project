namespace WebReckrytingSystem.Models
{
    public class SavedVacancy
    {
        public int Id { get; set; }
        public string StudentEmail { get; set; } = string.Empty;
        public string VacancyCompanyName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; } = DateTime.Now;
        public Vacancy Vacancy { get; set; }

    }
}