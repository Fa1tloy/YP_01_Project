using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WebReckrytingSystem.Models.Admin
{
    public class AdminResumeViewModel
    {
        [Required(ErrorMessage = "Желаемая должность обязательна")]
        [StringLength(255)]
        public string DesiredPosition { get; set; } = string.Empty;

        [Range(0, 9999999, ErrorMessage = "Зарплата от 0 до 9 999 999")]
        public int? SalaryExpectations { get; set; }

        [Required(ErrorMessage = "Город обязателен")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите готовность к командировкам")]
        [StringLength(20)]
        public string BusinessTripReadiness { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите статус поиска")]
        [StringLength(50)]
        public string SearchStatus { get; set; } = string.Empty;

        [Range(14, 100, ErrorMessage = "Возраст от 14 до 100")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "Укажите тип занятости")]
        [StringLength(50)]
        public string EmploymentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите график работы")]
        [StringLength(50)]
        public string WorkSchedule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Специальность обязательна")]
        [StringLength(255)]
        public string Specialty { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите пол")]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        public bool HasCar { get; set; }

        [StringLength(255)]
        public string? DriverLicenseCategory { get; set; }

        public string? ExperienceDescription { get; set; }

        // Строковое поле без сложной валидации
        public string? EducationDescription { get; set; }

        // Строковое поле без валидации List<string>
        public string? Skills { get; set; }

        public bool IsPublished { get; set; } = true;

        public List<PracticeViewModel> Practices { get; set; } = new();
    }
}
