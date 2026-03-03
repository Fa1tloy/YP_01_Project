using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class SavedResume
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string EmployerEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ResumeUserEmail { get; set; } = string.Empty;

        public DateTime SavedAt { get; set; } = DateTime.Now;
        public Resume Resume { get; set; }
    }
}