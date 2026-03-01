using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Pages.Vacancy;
using WebReckrytingSystem.Services;
using WebReckrytingSystem.Data;

namespace WebReckrytingSystem.Tests.Pages.Vacancy
{
    [TestClass]
    public class EditModelTests
    {
        private Mock<IVacancyService> _mockVacancyService;
        private Mock<ICompanyRepository> _mockCompanyRepository;
        private Mock<ILogger<EditModel>> _mockLogger;
        private EditModel _editModel;

        private readonly ClaimsPrincipal _employerUser = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "hr@techcorp.com"),
                new Claim(ClaimTypes.GivenName, "Ольга"),
                new Claim(ClaimTypes.Role, "employer")
            }, "TestAuth")
        );

        private readonly WebReckrytingSystem.Models.Vacancy _testVacancy = new WebReckrytingSystem.Models.Vacancy
        {
            CompanyName = "TechCorp",
            Title = "Senior .NET Developer",
            Description = "Разработка высоконагруженных приложений",
            Requirements = "Опыт работы 3+ года, знание C#, ASP.NET",
            SalaryFrom = 150000,
            SalaryTo = 250000,
            EmploymentType = "full",
            WorkSchedule = "full_day",
            AuthorEmail = "hr@techcorp.com"
        };



        [TestMethod]
        public void OnGet_ValidVacancy_ReturnsPageWithPrefilledData()
        {
            // Arrange
            _mockVacancyService.Setup(x => x.GetVacancy("TechCorp", "Senior .NET Developer"))
                .Returns(_testVacancy);
            _mockCompanyRepository.Setup(x => x.GetUserCompanies("hr@techcorp.com"))
                .Returns(new List<Company> { new Company { Name = "TechCorp" } });

            // Act
            var result = _editModel.OnGet("TechCorp", "Senior .NET Developer");

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsNotNull(_editModel.CurrentVacancy);
            Assert.AreEqual("Senior .NET Developer", _editModel.VacancyData.Title);
            Assert.AreEqual("TechCorp", _editModel.VacancyData.CompanyName);
            Assert.AreEqual(150000, _editModel.VacancyData.SalaryFrom);
        }

        [TestMethod]
        public void OnGet_VacancyNotFound_ReturnsPageWithErrorMessage()
        {
            // Arrange
            _mockVacancyService.Setup(x => x.GetVacancy("TechCorp", "NonExistent"))
                .Returns((WebReckrytingSystem.Models.Vacancy)null);

            // Act
            var result = _editModel.OnGet("TechCorp", "NonExistent");

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsFalse(string.IsNullOrEmpty(_editModel.ErrorMessage));
        }

        [TestMethod]
        public void OnGet_NotAuthor_ReturnsAccessDenied()
        {
            // Arrange
            var otherUserVacancy = new WebReckrytingSystem.Models.Vacancy
            {
                CompanyName = "TechCorp",
                Title = "Senior .NET Developer",
                AuthorEmail = "ceo@startup.com" // Другой автор
            };

            _mockVacancyService.Setup(x => x.GetVacancy("TechCorp", "Senior .NET Developer"))
                .Returns(otherUserVacancy);

            // Act
            var result = _editModel.OnGet("TechCorp", "Senior .NET Developer");

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
            var redirectResult = (RedirectToPageResult)result;
            Assert.AreEqual("/AccessDenied", redirectResult.PageName);
        }

        [TestMethod]
        public void OnPost_ValidData_ReturnsRedirectToDashboard()
        {
            // Arrange
            var updateModel = new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = "Lead .NET Developer",
                Description = "Руководство командой",
                Requirements = "Опыт руководства",
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            _editModel.VacancyData = updateModel;
            _editModel.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            var successResult = ServiceResult<WebReckrytingSystem.Models.Vacancy>.Success("Вакансия успешно обновлена!", _testVacancy);
            _mockVacancyService.Setup(x => x.UpdateVacancy(
                "TechCorp", "Senior .NET Developer", "hr@techcorp.com", updateModel))
                .Returns(successResult);

            // Act
            var result = _editModel.OnPost("TechCorp", "Senior .NET Developer");

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
            var redirectResult = (RedirectToPageResult)result;
            Assert.AreEqual("/Account/EmployerDashboard", redirectResult.PageName);
            Assert.IsTrue(_editModel.TempData.ContainsKey("SuccessMessage"));
        }

        [TestMethod]
        public void OnPost_InvalidModel_ReturnsPageWithErrors()
        {
            // Arrange
            _editModel.ModelState.AddModelError("Title", "Название обязательно");

            // Act
            var result = _editModel.OnPost("TechCorp", "Senior .NET Developer");

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsFalse(_editModel.ModelState.IsValid);
        }

        [TestMethod]
        public void OnPost_ServiceReturnsError_ReturnsPageWithErrorMessage()
        {
            // Arrange
            var updateModel = new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = "Lead .NET Developer",
                Description = "Описание",
                Requirements = "Требования",
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            _editModel.VacancyData = updateModel;

            var errorResult = ServiceResult<WebReckrytingSystem.Models.Vacancy>.Error("Название вакансии обязательно");
            _mockVacancyService.Setup(x => x.UpdateVacancy(
                "TechCorp", "Senior .NET Developer", "hr@techcorp.com", updateModel))
                .Returns(errorResult);

            _mockVacancyService.Setup(x => x.GetVacancy("TechCorp", "Senior .NET Developer"))
                .Returns(_testVacancy);
            _mockCompanyRepository.Setup(x => x.GetUserCompanies("hr@techcorp.com"))
                .Returns(new List<Company> { new Company { Name = "TechCorp" } });

            // Act
            var result = _editModel.OnPost("TechCorp", "Senior .NET Developer");

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsFalse(string.IsNullOrEmpty(_editModel.ErrorMessage));
            Assert.AreEqual("Название вакансии обязательно", _editModel.ErrorMessage);
        }

    }
        
}