// Models/Resume.cs
using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class Resume
    {
        [Key]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string DesiredPosition { get; set; } = string.Empty;

        public string? ExperienceDescription { get; set; }
        public string? EducationDescription { get; set; }
        public string? Skills { get; set; }

        [Range(0, 9999999)]
        public int? SalaryExpectations { get; set; }

        public string? City { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? BusinessTripReadiness { get; set; }
        public string? SearchStatus { get; set; }
        public string? EmploymentType { get; set; }
        public string? WorkSchedule { get; set; }
        public string? Specialty { get; set; }
        public bool? HasCar { get; set; }
        public string? DriverLicenseCategory { get; set; }

        public bool IsPublished { get; set; }

        // Навигационное свойство
        public User User { get; set; } = null!;
        public string? PracticesJson { get; set; }
    }
}
