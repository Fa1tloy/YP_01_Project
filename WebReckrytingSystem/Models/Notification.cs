using System.ComponentModel.DataAnnotations.Schema;

namespace WebReckrytingSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool IsRead { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
