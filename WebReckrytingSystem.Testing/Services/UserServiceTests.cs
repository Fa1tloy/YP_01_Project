using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Tests.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private UserService _userService;

        // Тестовые данные
        private readonly User _seeker1 = new User
        {
            Email = "petrov.ivan@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("S1234567"),
            FirstName = "Иван",
            LastName = "Петров",
            Role = "job_seeker"
        };

        private readonly User _seeker2 = new User
        {
            Email = "sidorova.maria@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("M7654321"),
            FirstName = "Мария",
            LastName = "Сидорова",
            Role = "job_seeker"
        };

        private readonly User _employer1 = new User
        {
            Email = "hr@techcompany.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Hr20241234"),
            FirstName = "Ольга",
            LastName = "Смирнова",
            Role = "employer"
        };

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();

            // Настройка mock репозитория с несколькими пользователями
            _mockUserRepository.Setup(repo => repo.FindByEmail("petrov.ivan@example.com"))
                .Returns(_seeker1);
            _mockUserRepository.Setup(repo => repo.FindByEmail("sidorova.maria@example.com"))
                .Returns(_seeker2);
            _mockUserRepository.Setup(repo => repo.FindByEmail("hr@techcompany.ru"))
                .Returns(_employer1);
            _mockUserRepository.Setup(repo => repo.FindByEmail("nonexistent@example.com"))
                .Returns((User)null);

            _userService = new UserService(_mockUserRepository.Object);
        }

    }
}