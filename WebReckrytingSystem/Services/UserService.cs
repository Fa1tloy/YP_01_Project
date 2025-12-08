using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using System.Linq;

namespace WebReckrytingSystem.Services
{
    /// <summary>
    /// Сервис для работы с пользователями (регистрация, аутентификация)
    /// </summary>
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public ServiceResult<User> RegisterUser(
            string email,
            string password,
            string firstName,
            string lastName,
            string role,
            string? companyName = null)
        {
            // Валидация обязательных полей
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

            // Для работодателя проверяем наличие названия компании
            if (role == "employer" && string.IsNullOrWhiteSpace(companyName))
                return ServiceResult<User>.Error("Для работодателя обязательно указать название компании");

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
                else
                {
                    _logger.LogInformation("Компания {CompanyName} уже существует, используем существующую", companyName);
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
                    Role = role,
                    CompanyName = role == "employer" ? companyName?.Trim() : null // === ПРИВЯЗЬ К КОМПАНИИ ТОЛЬКО ДЛЯ РАБОТОДАТЕЛЕЙ ===
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

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        public ServiceResult<User> AuthenticateUser(string email, string password)
        {
            _logger.LogInformation("🔐 Аутентификация пользователя: {Email}", email);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("⚠️ Пустые Email или пароль");
                return ServiceResult<User>.Error("Email и пароль обязательны для заполнения");
            }

            var user = _userRepository.FindByEmail(email);
            if (user == null)
            {
                _logger.LogWarning("⚠️ Пользователь не найден: {Email}", email);
                return ServiceResult<User>.Error("Неверный email или пароль");
            }

            // Проверка пароля
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("⚠️ Неверный пароль для {Email}", email);
                return ServiceResult<User>.Error("Неверный email или пароль");
            }

            _logger.LogInformation("✅ Успешный вход: {Email}", email);
            return ServiceResult<User>.Success("Успешный вход", user);
        }

        /// <summary>
        /// Валидация email формата
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return Regex.IsMatch(email, pattern);
        }
    }
}