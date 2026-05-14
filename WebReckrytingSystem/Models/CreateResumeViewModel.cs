using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class CreateResumeViewModel
    {
        [Required(ErrorMessage = "Желаемая должность обязательна для заполнения")]
        [StringLength(255, ErrorMessage = "Желаемая должность не должна превышать 255 символов")]
        [Display(Name = "Желаемая должность")]
        public string DesiredPosition { get; set; } = string.Empty;

        [Required(ErrorMessage = "Город обязателен для заполнения")]
        [StringLength(100, ErrorMessage = "Город не должен превышать 100 символов")]
        [Display(Name = "Город")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите готовность к командировкам")]
        [StringLength(20)]
        [Display(Name = "Готовность к командировкам")]
        public string BusinessTripReadiness { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите статус поиска")]
        [StringLength(50)]
        [Display(Name = "Статус поиска")]
        public string SearchStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Возраст обязателен для заполнения")]
        [Range(14, 100, ErrorMessage = "Возраст должен быть в диапазоне от 14 до 100")]
        [Display(Name = "Возраст")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "Укажите тип занятости")]
        [StringLength(50)]
        [Display(Name = "Тип занятости")]
        public string EmploymentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите график работы")]
        [StringLength(50)]
        [Display(Name = "График работы")]
        public string WorkSchedule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Специальность обязательна для заполнения")]
        [StringLength(255, ErrorMessage = "Специальность не должна превышать 255 символов")]
        [Display(Name = "Специальность")]
        public string Specialty { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите пол")]
        [StringLength(20)]
        [Display(Name = "Пол")]
        public string Gender { get; set; } = string.Empty;

        [Range(0, 9999999, ErrorMessage = "Зарплата должна быть в диапазоне от 0 до 9 999 999")]
        [Display(Name = "Зарплатные ожидания")]
        public int? SalaryExpectations { get; set; }

        [Display(Name = "Наличие автомобиля")]
        public bool HasCar { get; set; }

        [StringLength(255, ErrorMessage = "Категория прав не должна превышать 255 символов")]
        [Display(Name = "Категория прав")]
        public string? DriverLicenseCategory { get; set; }

        [Display(Name = "Категории прав")]
        public List<string> DriverLicenseCategories { get; set; } = new();

        [Display(Name = "Опыт работы (лет)")]
        [Range(0, 60, ErrorMessage = "Опыт работы не может быть отрицательным или больше 60 лет")]
        public int? ExperienceYears { get; set; }

        [StringLength(1000, ErrorMessage = "Описание опыта не должно превышать 1000 символов")]
        [Display(Name = "Описание опыта работы")]
        public string? ExperienceDescription { get; set; }

        // Образование – только учебное заведение и год окончания
        [Required(ErrorMessage = "Укажите учебное заведение")]
        [StringLength(255, ErrorMessage = "Название учебного заведения не должно превышать 255 символов")]
        [Display(Name = "Учебное заведение")]
        public string EducationalInstitution { get; set; } = "ФГБОУ Колледж Росрезерва";

        [Range(1900, 2025, ErrorMessage = "Год окончания должен быть между 1900 и текущим годом")]
        [Display(Name = "Год окончания")]
        public int? GraduationYear { get; set; }

        // Навыки
        public List<string> Skills { get; set; } = new();

        [Display(Name = "Опубликовать резюме")]
        public bool IsPublished { get; set; } = true;

        // Практики
        public List<PracticeViewModel> Practices { get; set; } = new();
    }
}