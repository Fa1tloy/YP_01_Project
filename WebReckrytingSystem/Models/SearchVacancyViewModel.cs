// Models/SearchVacancyViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class SearchVacancyViewModel
    {
        [StringLength(100, ErrorMessage = "Ключевые слова не должны превышать 100 символов")]
        [Display(Name = "Ключевые слова")]
        public string? Keywords { get; set; }

        [StringLength(255, ErrorMessage = "Название компании не должно превышать 255 символов")]
        [Display(Name = "Компания")]
        public string? CompanyName { get; set; }

        [StringLength(100, ErrorMessage = "Регион не должен превышать 100 символов")]
        [Display(Name = "Регион")]
        public string? Region { get; set; }

        [Range(0, 9999999, ErrorMessage = "Зарплата должна быть в диапазоне от 0 до 9 999 999")]
        [Display(Name = "Зарплата от (руб.)")]
        public int? SalaryFrom { get; set; }

        [Display(Name = "Тип занятости")]
        public string? EmploymentType { get; set; }

        [Display(Name = "График работы")]
        public string? WorkSchedule { get; set; }

        [Range(1, 24, ErrorMessage = "Рабочие часы в день должны быть от 1 до 24")]
        [Display(Name = "Рабочие часы в день")]
        public int? WorkHoursPerDay { get; set; }

        [Display(Name = "Формат работы")]
        public string? WorkFormat { get; set; }

        [Display(Name = "Специальность")]
        public string? Specialty { get; set; }

        [Range(1, 100, ErrorMessage = "Номер страницы должен быть от 1 до 100")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Размер страницы должен быть от 1 до 50")]
        public int PageSize { get; set; } = 10;

        [Display(Name = "Тип вакансии")]
        public bool? IsPracticum { get; set; }  // null = все, true = практика, false = без практики
    }
}