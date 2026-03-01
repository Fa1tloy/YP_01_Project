using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class CreateVacancyViewModel
    {
        [Required(ErrorMessage = "Название компании обязательно")]
        [StringLength(255, ErrorMessage = "Название компании не должно превышать 255 символов")]
        [Display(Name = "Название компании")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Регион не должен превышать 100 символов")]
        [Display(Name = "Регион")]
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "Название вакансии обязательно")]
        [StringLength(255, ErrorMessage = "Название вакансии не должно превышать 255 символов")]
        [Display(Name = "Должность/название вакансии")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание вакансии обязательно")]
        [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
        [Display(Name = "Описание вакансии")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Требования к кандидату обязательны")]
        [StringLength(2000, ErrorMessage = "Требования не должны превышать 2000 символов")]
        [Display(Name = "Требования к кандидату")]
        public string Requirements { get; set; } = string.Empty;

        [Range(0, 9999999, ErrorMessage = "Доход должен быть в диапазоне от 0 до 9 999 999")]
        [Display(Name = "Доход от (руб.)")]
        public int? SalaryFrom { get; set; }

        [Range(0, 9999999, ErrorMessage = "Зарплата должна быть в диапазоне от 0 до 9 999 999")]
        [Display(Name = "Зарплата до (руб.)")]
        public int? SalaryTo { get; set; }

        [Required(ErrorMessage = "Тип занятости обязателен")]
        [Display(Name = "Тип занятости")]
        public string EmploymentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "График работы обязателен")]
        [Display(Name = "График работы")]
        public string WorkSchedule { get; set; } = string.Empty;

        [Range(1, 24, ErrorMessage = "Рабочие часы в день должны быть от 1 до 24")]
        [Display(Name = "Рабочие часы в день")]
        public int? WorkHoursPerDay { get; set; }

        [Display(Name = "Формат работы")]
        [StringLength(50)]
        public string WorkFormat { get; set; } = string.Empty;

        [Display(Name = "Период дохода")]
        [StringLength(20)]
        public string SalaryPeriod { get; set; } = string.Empty;

        [Display(Name = "Частота выплат")]
        [StringLength(50)]
        public string PaymentFrequency { get; set; } = string.Empty;

        [Display(Name = "Специальность")]
        [StringLength(255)]
        public string Specialty { get; set; } = string.Empty;
    }
}
