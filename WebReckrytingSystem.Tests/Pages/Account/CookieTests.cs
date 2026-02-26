using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    public class CookieTests
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

        

        

        

        [TestMethod]
        public async Task LogoutModel_OnPostAsync_RemovesCookie()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(auth => auth.SignOutAsync(It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(provider => provider.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProviderMock.Object;

            var logoutModel = new LogoutModel()
            {
                PageContext = new PageContext
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = await logoutModel.OnPostAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
            var redirectResult = (RedirectToPageResult)result;
            Assert.AreEqual("/Index", redirectResult.PageName);

            authServiceMock.Verify(auth => auth.SignOutAsync(
                It.IsAny<HttpContext>(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
                Times.Once);
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
    }
}