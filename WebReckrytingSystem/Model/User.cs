using System.ComponentModel.DataAnnotations;

namespace WebReckrytingSystem.Model
{
    public class User
    {
        public required string Email { get; set; }


        public required string PasswordHash { get; set; }

        [Required(ErrorMessage = "Имя обязательно для заполнения.")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов.")]
        public required string FirstName { get; set; }


        [Required(ErrorMessage = "Фамилия обязательна для заполнения.")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов.")]
        public required string LastName { get; set; }

        
        public UserRole Role { get; set; }




    }
}
