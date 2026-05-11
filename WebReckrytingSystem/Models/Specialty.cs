using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
    }
}