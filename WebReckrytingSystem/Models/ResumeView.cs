namespace WebReckrytingSystem.Models
{
    public class ResumeView
    {
        public int Id { get; set; }
        public string ResumeEmail { get; set; } = string.Empty;
        public string ViewerEmail { get; set; } = string.Empty;
        public DateTime ViewedAt { get; set; } = DateTime.Now;
        public string? ViewedFromIp { get; set; }
    }
}