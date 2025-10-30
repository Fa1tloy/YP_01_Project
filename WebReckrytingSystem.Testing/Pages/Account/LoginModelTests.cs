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

        [TestInitialize]
        public void Setup()
        {
            _mockUserService = new Mock<UserService>(Mock.Of<IUserRepository>());
            _loginModel = new LoginModel(_mockUserService.Object)
            {
                PageContext = new PageContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        
    }
}