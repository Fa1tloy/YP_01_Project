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

       

        [TestMethod]
        public void AuthenticateUser_ValidCredentialsSeeker1_ReturnsSuccess()
        {
            // Act
            var result = _userService.AuthenticateUser("petrov.ivan@example.com", "S1234567");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Успешный вход", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("petrov.ivan@example.com", result.Data.Email);
            Assert.AreEqual("job_seeker", result.Data.Role);
        }

        [TestMethod]
        public void AuthenticateUser_ValidCredentialsSeeker2_ReturnsSuccess()
        {
            // Act
            var result = _userService.AuthenticateUser("sidorova.maria@example.com", "M7654321");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Успешный вход", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("sidorova.maria@example.com", result.Data.Email);
            Assert.AreEqual("job_seeker", result.Data.Role);
        }

        [TestMethod]
        public void AuthenticateUser_ValidCredentialsEmployer_ReturnsSuccess()
        {
            // Act
            var result = _userService.AuthenticateUser("hr@techcompany.ru", "Hr20241234");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Успешный вход", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("hr@techcompany.ru", result.Data.Email);
            Assert.AreEqual("employer", result.Data.Role);
        }

        [TestMethod]
        public void AuthenticateUser_InvalidEmail_ReturnsError()
        {
            // Act
            var result = _userService.AuthenticateUser("nonexistent@example.com", "AnyPassword123");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Неверный email или пароль", result.Message);
            Assert.IsNull(result.Data);
        }

        [TestMethod]
        public void AuthenticateUser_InvalidPassword_ReturnsError()
        {
            // Act
            var result = _userService.AuthenticateUser("petrov.ivan@example.com", "WrongPassword123");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Неверный email или пароль", result.Message);
            Assert.IsNull(result.Data);
        }

        [TestMethod]
        public void AuthenticateUser_EmptyEmail_ReturnsError()
        {
            // Act
            var result = _userService.AuthenticateUser("", "S1234567");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Email и пароль обязательны для заполнения", result.Message);
            Assert.IsNull(result.Data);
        }

        [TestMethod]
        public void AuthenticateUser_EmptyPassword_ReturnsError()
        {
            // Act
            var result = _userService.AuthenticateUser("petrov.ivan@example.com", "");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Email и пароль обязательны для заполнения", result.Message);
            Assert.IsNull(result.Data);
        }
    }
}