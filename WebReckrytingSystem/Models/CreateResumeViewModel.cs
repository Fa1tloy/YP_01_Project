using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class CreateResumeViewModel
    {
        [Required(ErrorMessage = "Желаемая должность обязательна для заполнения")]
        [StringLength(255, ErrorMessage = "Желаемая должность не должна превышать 255 символов")]
        [Display(Name = "Желаемая должность")]
        public string DesiredPosition { get; set; } = string.Empty;

        [Range(0, 9999999, ErrorMessage = "Зарплата должна быть в диапазоне от 0 до 9 999 999")]
        [Display(Name = "Зарплатные ожидания")]
        public int? SalaryExpectations { get; set; }

        [Display(Name = "Опыт работы (лет)")]
        [Range(0, 60, ErrorMessage = "Опыт работы не может быть отрицательным или больше 60 лет")]
        public int? ExperienceYears { get; set; }

        [StringLength(1000, ErrorMessage = "Описание опыта не должно превышать 1000 символов")]
        [Display(Name = "Описание опыта работы")]
        public string? ExperienceDescription { get; set; }

        // Образование
        [Required(ErrorMessage = "Укажите учебное заведение")]
        [StringLength(255, ErrorMessage = "Название учебного заведения не должно превышать 255 символов")]
        [Display(Name = "Учебное заведение")]
        public string EducationalInstitution { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Факультет не должен превышать 255 символов")]
        [Display(Name = "Факультет")]
        public string? Faculty { get; set; }

        [StringLength(255, ErrorMessage = "Специализация не должна превышать 255 символов")]
        [Display(Name = "Специализация")]
        public string? Specialization { get; set; }

        [Range(1900, 2025, ErrorMessage = "Год окончания должен быть между 1900 и текущим годом")]
        [Display(Name = "Год окончания")]
        public int? GraduationYear { get; set; }

        // Навыки
        public List<string> Skills { get; set; } = new(); // оставлено для совместимости

        [Display(Name = "Опубликовать резюме")]
        public bool IsPublished { get; set; } = true;

        // Практики
        public List<PracticeViewModel> Practices { get; set; } = new();
    }
}