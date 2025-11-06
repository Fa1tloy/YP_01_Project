// Services/UserService.cs
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public ServiceResult AuthenticateUser(string email, string password)
    {
        // 1. Валидация входных данных
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return ServiceResult.Error("Email и пароль обязательны для заполнения");

        // 2. Поиск пользователя в БД
        var user = _userRepository.FindByEmail(email);
        if (user == null)
            return ServiceResult.Error("Неверный email или пароль");

        // 3. Проверка пароля с BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return ServiceResult.Error("Неверный email или пароль");

        // 4. Успешная аутентификация
        return ServiceResult.Success("Успешный вход", user);
    }

    public ServiceResult RegisterUser(string email, string password, string firstName, string lastName, string role)
    {
        // 1. Проверка уникальности email
        if (_userRepository.FindByEmail(email) != null)
            return ServiceResult.Error("Пользователь с таким email уже зарегистрирован");

        // 2. Хеширование пароля
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // 3. Создание пользователя
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };

        // 4. Сохранение в БД
        var savedUser = _userRepository.Save(user);
        return ServiceResult.Success("Пользователь успешно зарегистрирован", savedUser);
    }
}