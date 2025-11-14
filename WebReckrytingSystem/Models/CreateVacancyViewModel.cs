using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class CreateVacancyViewModel
    {
        [Required(ErrorMessage = "Название компании обязательно")]
        [StringLength(255, ErrorMessage = "Название компании не должно превышать 255 символов")]
        [Display(Name = "Название компании")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Название вакансии обязательно")]
        [StringLength(255, ErrorMessage = "Название вакансии не должно превышать 255 символов")]
        [Display(Name = "Должность/название вакансии")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание вакансии обязательно")]
        [Display(Name = "Описание вакансии")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Требования к кандидату обязательны")]
        [Display(Name = "Требования к кандидату")]
        public string Requirements { get; set; } = string.Empty;

        [Range(0, 9999999, ErrorMessage = "Зарплата должна быть в диапазоне от 0 до 9 999 999")]
        [Display(Name = "Зарплата от (руб.)")]
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
    }
}