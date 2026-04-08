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

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(20)]
        public string BusinessTripReadiness { get; set; } = string.Empty;

        [StringLength(50)]
        public string SearchStatus { get; set; } = string.Empty;

        [Range(14, 100)]
        public int? Age { get; set; }

        [StringLength(50)]
        public string EmploymentType { get; set; } = string.Empty;

        [StringLength(50)]
        public string WorkSchedule { get; set; } = string.Empty;

        [StringLength(255)]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        public string? ExperienceDescription { get; set; }
        public string? EducationDescription { get; set; }
        public string? Skills { get; set; }

        [Range(0, 9999999)]
        public int? SalaryExpectations { get; set; }

        public bool HasCar { get; set; }

        [StringLength(255)]
        public string? DriverLicenseCategory { get; set; }

        public bool IsPublished { get; set; }

        // Навигационное свойство
        public User User { get; set; } = null!;
        public string? PracticesJson { get; set; }
    }
}
