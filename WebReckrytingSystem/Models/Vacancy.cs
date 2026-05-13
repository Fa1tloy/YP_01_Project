// Models/Vacancy.cs
using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class Vacancy
    {
        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Requirements { get; set; } = string.Empty;

        public int? SalaryFrom { get; set; }

        [Required]
        public string EmploymentType { get; set; } = string.Empty;

        [Required]
        public string WorkSchedule { get; set; } = string.Empty;

        public int? WorkHoursPerDay { get; set; }

        [StringLength(50)]
        public string WorkFormat { get; set; } = string.Empty;

        [StringLength(255)]
        public string Specialty { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string AuthorEmail { get; set; } = string.Empty;

        public bool IsPracticum { get; set; }  // новое поле

        public Company Company { get; set; } = null!;
        public User Author { get; set; } = null!;
    }
}