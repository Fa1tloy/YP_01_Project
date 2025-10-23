using System.Text.RegularExpressions;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ServiceResult RegisterUser(
            string email,
            string password,
            string firstName,
            string lastName,
            string role)
        {
            // Валидация email формата
            if (!IsValidEmail(email))
                return ServiceResult.Error("Неверный формат email");

            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return ServiceResult.Error("Имя и фамилия обязательны для заполнения");

            // Валидация пароля
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return ServiceResult.Error("Пароль должен содержать минимум 8 символов");

            // Валидация роли (теперь проверяем строки)
            if (role != "job_seeker" && role != "employer")
                return ServiceResult.Error("Неверно указана роль. Допустимые значения: job_seeker, employer");

            // Проверка уникальности email
            var existingUser = _userRepository.FindByEmail(email);
            if (existingUser != null)
                return ServiceResult.Error("Email уже зарегистрирован");

            try
            {
                // Создание нового пользователя
                var newUser = new User
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    Role = role // Теперь просто string
                };

                // Сохранение в БД
                var savedUser = _userRepository.Save(newUser);

                return ServiceResult.Success("Пользователь успешно зарегистрирован!", savedUser);
            }
            catch (Exception ex)
            {
                return ServiceResult.Error($"Ошибка при сохранении пользователя: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return Regex.IsMatch(email, pattern);
        }
    }
}