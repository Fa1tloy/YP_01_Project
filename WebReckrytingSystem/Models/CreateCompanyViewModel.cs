using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Models
{
    public class CreateCompanyViewModel
    {
        [Required(ErrorMessage = "Название компании обязательно")]
        [StringLength(255, ErrorMessage = "Название компании не должно превышать 255 символов")]
        [Display(Name = "Название компании")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        [Display(Name = "Описание компании")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Введите корректный URL")]
        [Display(Name = "Веб-сайт")]
        public string? Website { get; set; }
    }
}