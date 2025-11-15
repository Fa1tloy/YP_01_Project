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
    }
}
