using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebReckrytingSystem.Models
{
    public class Vacancy
    {
        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Requirements { get; set; } = string.Empty;

        public int? SalaryFrom { get; set; }

        public int? SalaryTo { get; set; }

        [Required]
        public string EmploymentType { get; set; } = string.Empty;

        [Required]
        public string WorkSchedule { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string AuthorEmail { get; set; } = string.Empty;

        // Навигационные свойства
        public Company Company { get; set; } = null!;
        public User Author { get; set; } = null!;
    }
}