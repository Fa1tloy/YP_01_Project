using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class CompanySettingsViewModel
    {
        [Required(ErrorMessage = "Название компании обязательно")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Введите корректный URL")]
        public string? Website { get; set; }

        public string? LogoUrl { get; set; }
    }
}