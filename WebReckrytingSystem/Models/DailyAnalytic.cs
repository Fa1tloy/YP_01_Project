using System.ComponentModel.DataAnnotations.Schema;

namespace WebReckrytingSystem.Models
{
    public class DailyAnalytic
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;

        [Column("Data")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int ProfileViews { get; set; }
        public int ApplicationsSent { get; set; }
        public int SavedVacancies { get; set; }
    }
}