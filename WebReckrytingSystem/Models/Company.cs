using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class Company
    {
        [Key]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Website { get; set; }

        public string? LogoUrl { get; set; }

        public bool Verified { get; set; } = false;

        // Навигационные свойства
        public ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
    }
}