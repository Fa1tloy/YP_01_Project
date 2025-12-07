using System.Text.RegularExpressions;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using Microsoft.Extensions.Logging;

namespace WebReckrytingSystem.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository; // Добавьте если нужно создавать компанию
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ICompanyRepository companyRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public ServiceResult<User> RegisterUser(
            string email,
            string password,
            string firstName,
            string lastName,
            string role,
            string? companyName = null) // Добавлен параметр
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return ServiceResult<User>.Error("Имя и фамилия обязательны для заполнения");

            // Валидация email формата
            if (!IsValidEmail(email))
                return ServiceResult<User>.Error("Неверный формат email");

            // Валидация пароля
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return ServiceResult<User>.Error("Пароль должен содержать минимум 8 символов");

            // Валидация роли
            if (role != "job_seeker" && role != "employer")
                return ServiceResult<User>.Error("Неверно указана роль. Допустимые значения: job_seeker, employer");

            // Проверка уникальности email
            var existingUser = _userRepository.FindByEmail(email);
            if (existingUser != null)
            {
                return ServiceResult<User>.Error($"Пользователь с email '{email}' уже зарегистрирован. Перейдите на страницу входа.");
            }

            // Для работодателя - проверяем/создаем компанию
            if (role == "employer" && !string.IsNullOrWhiteSpace(companyName))
            {
                var existingCompany = _companyRepository.FindByName(companyName.Trim());
                if (existingCompany == null)
                {
                    // Создаем компанию автоматически
                    var newCompany = new Company
                    {
                        Name = companyName.Trim(),
                        Verified = false,
                        Description = $"Компания {companyName}"
                    };
                    _companyRepository.Save(newCompany);
                    _logger.LogInformation("✅ Компания {CompanyName} создана автоматически", companyName);
                }
            }

            try
            {
                // Создание пользователя
                var newUser = new User
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    Role = role
                };

                var savedUser = _userRepository.Save(newUser);

                _logger.LogInformation("✅ Пользователь {Email} успешно зарегистрирован", email);

                return ServiceResult<User>.Success("Пользователь успешно зарегистрирован!", savedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при регистрации пользователя {Email}", email);
                return ServiceResult<User>.Error("Произошла ошибка при регистрации. Попробуйте позже.");
            }
        }

        public ServiceResult<User> AuthenticateUser(string email, string password)
        {
            _logger.LogInformation("🔐 Аутентификация пользователя: {Email}", email);

            // Проверка на пустые данные
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("⚠️ Пустые Email или пароль");
                return ServiceResult<User>.Error("Email и пароль обязательны для заполнения");
            }

            // Поиск пользователя
            var user = _userRepository.FindByEmail(email);
            if (user == null)
            {
                _logger.LogWarning("⚠️ Пользователь не найден: {Email}", email);
                return ServiceResult<User>.Error("Неверный email или пароль");
            }

            try
            {
                // Проверка пароля
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("⚠️ Неверный пароль для {Email}", email);
                    return ServiceResult<User>.Error("Неверный email или пароль");
                }

                _logger.LogInformation("✅ Успешный вход: {Email}", email);
                return ServiceResult<User>.Success("Успешный вход", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка проверки пароля для {Email}", email);
                return ServiceResult<User>.Error("Произошла ошибка при аутентификации");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
            return Regex.IsMatch(email, pattern);
        }
    }
}