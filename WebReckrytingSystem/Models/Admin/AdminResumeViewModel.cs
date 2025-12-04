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

        public string? ExperienceDescription { get; set; }

        // Строковое поле без сложной валидации
        public string? EducationDescription { get; set; }

        // Строковое поле без валидации List<string>
        public string? Skills { get; set; }

        public bool IsPublished { get; set; } = true;

        public List<PracticeViewModel> Practices { get; set; } = new();
    }
}