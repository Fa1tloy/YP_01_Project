namespace WebReckrytingSystem.Models
{
    public class DailyAnalytic
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int ProfileViews { get; set; }
        public int ApplicationsSent { get; set; }
        public int SavedVacancies { get; set; }
    }
}