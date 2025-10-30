using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using WebReckrytingSystem.Pages.Account;

namespace WebReckrytingSystem.Tests.Pages.Account
{
    [TestClass]
    public class AccessControlTests
    {
        [TestMethod]
        public void JobSeekerDashboard_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange
            var pageModel = new JobSeekerDashboardModel(Mock.Of<WebReckrytingSystem.Services.IResumeService>())
            {
                PageContext = new PageContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = pageModel.OnGet();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
            var redirectResult = (RedirectToPageResult)result;
            Assert.AreEqual("/Account/Login", redirectResult.PageName);
        }

        [TestMethod]
        public void EmployerDashboard_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange
            var pageModel = new EmployerDashboardModel()
            {
                PageContext = new PageContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            pageModel.OnGet();

            // Assert
            Assert.AreEqual("Пользователь", pageModel.UserFirstName);
        }

        [TestMethod]
        public void JobSeekerDashboard_AuthenticatedSeeker_ReturnsPage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "seeker@test.com"),
                new Claim(ClaimTypes.GivenName, "Иван"),
                new Claim(ClaimTypes.Role, "job_seeker")
            }, "TestAuth"));

            httpContext.User = user;

            var pageModel = new JobSeekerDashboardModel(
                Mock.Of<WebReckrytingSystem.Services.IResumeService>())
            {
                PageContext = new PageContext
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = pageModel.OnGet();

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.AreEqual("Иван", pageModel.UserFirstName);
        }

        [TestMethod]
        public void EmployerDashboard_AuthenticatedEmployer_ReturnsPage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "employer@test.com"),
                new Claim(ClaimTypes.GivenName, "Ольга"),
                new Claim(ClaimTypes.Role, "employer")
            }, "TestAuth"));

            httpContext.User = user;

            var pageModel = new EmployerDashboardModel()
            {
                PageContext = new PageContext
                {
                    HttpContext = httpContext
                }
            };

            // Act
            pageModel.OnGet();

            // Assert
            Assert.AreEqual("Ольга", pageModel.UserFirstName);
        }
    }
}