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
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return ServiceResult.Error("Имя и фамилия обязательны для заполнения");

            // Валидация email формата
            if (!IsValidEmail(email))
                return ServiceResult.Error("Неверный формат email");

            // Валидация пароля
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return ServiceResult.Error("Пароль должен содержать минимум 8 символов");

            // Валидация роли
            if (role != "job_seeker" && role != "employer")
                return ServiceResult.Error("Неверно указана роль. Допустимые значения: job_seeker, employer");

            // Проверка уникальности email (более детальная)
            var existingUser = _userRepository.FindByEmail(email);
            if (existingUser != null)
            {
                return ServiceResult.Error($"Пользователь с email '{email}' уже зарегистрирован. " +
                                         "Если это ваш аккаунт, перейдите на страницу входа.");
            }

            try
            {
                // Создание нового пользователя
                var newUser = new User
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    Role = role
                };

                // Сохранение в БД
                var savedUser = _userRepository.Save(newUser);

                return ServiceResult.Success("Пользователь успешно зарегистрирован!", savedUser);
            }
            catch (Exception ex)
            {
                // Логируем ошибку для разработчика
                Console.WriteLine($"Ошибка при регистрации пользователя: {ex.Message}");
                return ServiceResult.Error("Произошла ошибка при регистрации. Пожалуйста, попробуйте позже.");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return Regex.IsMatch(email, pattern);
        }

        public ServiceResult AuthenticateUser(string email, string password)
        {
            // Найти пользователя по email
            var user = _userRepository.FindByEmail(email);
            if (user == null)
                return ServiceResult.Error("Пользователь с таким email не найден");

            // Проверить пароль
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return ServiceResult.Error("Неверный пароль");

            return ServiceResult.Success("Успешный вход", user);
        }
    }
}