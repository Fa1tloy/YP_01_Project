using System.ComponentModel.DataAnnotations.Schema;

namespace WebReckrytingSystem.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? VacancyCompanyName { get; set; }
        public string? VacancyTitle { get; set; }

        [Column("SentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
    }
}
