using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Pages.Account;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Tests.Pages.Account
{
    [TestClass]
    public class LoginModelTests
    {
        private Mock<UserService> _mockUserService;
        private LoginModel _loginModel;

        private readonly User _seeker = new User
        {
            Email = "petrov.ivan@example.com",
            FirstName = "Иван",
            LastName = "Петров",
            Role = "job_seeker"
        };

        private readonly User _employer = new User
        {
            Email = "hr@techcompany.ru",
            FirstName = "Ольга",
            LastName = "Смирнова",
            Role = "employer"
        };

        

        

        

        

        [TestMethod]
        public async Task OnPostAsync_InvalidModelState_ReturnsPageWithValidationErrors()
        {
            // Arrange
            _loginModel.ModelState.AddModelError("Email", "Email обязателен");
            _loginModel.LoginData = new LoginViewModel
            {
                Email = "",
                Password = "password"
            };

            // Act
            var result = await _loginModel.OnPostAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            _mockUserService.Verify(service => service.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private Mock<IAuthenticationService> SetupAuthenticationService()
        {
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(auth => auth.SignInAsync(It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(provider => provider.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            _loginModel.PageContext.HttpContext.RequestServices = serviceProviderMock.Object;
            return authServiceMock;
        }

        private void VerifySignInCalled(Mock<IAuthenticationService> authServiceMock, string email, string role)
        {
            authServiceMock.Verify(auth => auth.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.Is<ClaimsPrincipal>(p =>
                    p.HasClaim(ClaimTypes.Email, email) &&
                    p.HasClaim(ClaimTypes.Role, role)),
                It.IsAny<AuthenticationProperties>()),
                Times.Once);
        }
    }
}