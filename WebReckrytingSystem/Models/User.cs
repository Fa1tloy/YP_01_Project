using System.ComponentModel.DataAnnotations;


namespace WebReckrytingSystem.Models
{
    public class User
    {
        [Key]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя обязательно для заполнения.")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна для заполнения.")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public  string Role { get; set; }

        public string? AvatarUrl { get; set; }

        // === ДОБАВЛЯЕМ СВЯЗЬ С КОМПАНИЕЙ ===
        public string? CompanyName { get; set; }

        // === НАВИГАЦИОННОЕ СВОЙСТВО ===
        public Company? Company { get; set; }
        // Константы для ролей
        public const string ROLE_JOB_SEEKER = "job_seeker";
        public const string ROLE_EMPLOYER = "employer";
        public const string ROLE_ADMIN = "admin";
    }
}
