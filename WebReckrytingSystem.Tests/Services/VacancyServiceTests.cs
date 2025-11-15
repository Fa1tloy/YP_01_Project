using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Tests.Services
{
    [TestClass]
    public class VacancyServiceTests
    {
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<ICompanyRepository> _mockCompanyRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<VacancyService>> _mockLogger;
        private readonly VacancyService _vacancyService;

        // Тестовые данные
        private readonly User _employerUser = new User
        {
            Email = "hr@techcorp.com",
            FirstName = "Ольга",
            LastName = "Смирнова",
            Role = "employer"
        };

        private readonly User _jobSeekerUser = new User
        {
            Email = "petrov.ivan@example.com",
            FirstName = "Иван",
            LastName = "Петров",
            Role = "job_seeker"
        };

        private readonly Company _testCompany = new Company
        {
            Name = "TechCorp",
            Description = "IT компания",
            Verified = true
        };

        public VacancyServiceTests()
        {
            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockCompanyRepository = new Mock<ICompanyRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<VacancyService>>();

            _vacancyService = new VacancyService(
                _mockVacancyRepository.Object,
                _mockCompanyRepository.Object,
                _mockUserRepository.Object);
        }

        private CreateVacancyViewModel CreateValidVacancyModel()
        {
            return new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = "Senior .NET Developer",
                Description = "Разработка высоконагруженных приложений",
                Requirements = "Опыт от 3 лет, C#, ASP.NET, SQL",
                SalaryFrom = 150000,
                SalaryTo = 250000,
                EmploymentType = "full",
                WorkSchedule = "remote"
            };
        }
        [TestMethod]
        public void CreateVacancy_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);
            _mockVacancyRepository.Setup(x => x.GetByCompanyAndTitle(model.CompanyName, model.Title))
                .Returns((Vacancy)null);
            _mockVacancyRepository.Setup(x => x.Save(It.IsAny<Vacancy>()))
                .Returns((Vacancy v) => v);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Вакансия успешно опубликована!", result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(model.Title, result.Data.Title);
            Assert.AreEqual(model.CompanyName, result.Data.CompanyName);
            _mockVacancyRepository.Verify(x => x.Save(It.IsAny<Vacancy>()), Times.Once);
        }

        [TestMethod]
        public void CreateVacancy_WithMinimumRequiredData_ReturnsSuccess()
        {
            // Arrange
            var model = new CreateVacancyViewModel
            {
                CompanyName = "StartupInnovations",
                Title = "Frontend Developer",
                Description = "Разработка UI",
                Requirements = "JavaScript, React",
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            var company = new Company { Name = "StartupInnovations", Verified = false };

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(company);
            _mockVacancyRepository.Setup(x => x.GetByCompanyAndTitle(model.CompanyName, model.Title))
                .Returns((Vacancy)null);
            _mockVacancyRepository.Setup(x => x.Save(It.IsAny<Vacancy>()))
                .Returns((Vacancy v) => v);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Data.SalaryFrom);
            Assert.IsNull(result.Data.SalaryTo);
        }
    }
}
