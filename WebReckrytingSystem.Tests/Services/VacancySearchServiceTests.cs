using Microsoft.Extensions.Logging;
using Moq;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace UnitTests.Services
{
    [TestClass]
    public class VacancySearchServiceTests
    {
        private readonly Mock<IVacancyRepository> _mockVacancyRepository;
        private readonly Mock<ILogger<VacancySearchService>> _mockLogger;
        private readonly VacancySearchService _vacancySearchService;

        // Тестовые данные
        private readonly List<Vacancy> _testVacancies = new()
        {
            new Vacancy
            {
                CompanyName = "TechCorp",
                Title = "Senior .NET Developer",
                Description = "Разработка высоконагруженных приложений на C# и ASP.NET",
                Requirements = "Опыт работы с .NET, C#, SQL Server, Entity Framework",
                SalaryFrom = 150000,
                SalaryTo = 250000,
                EmploymentType = "full",
                WorkSchedule = "remote",
                AuthorEmail = "hr@techcorp.com"
            },
            new Vacancy
            {
                CompanyName = "StartupInnovations",
                Title = "Frontend React Developer",
                Description = "Разработка пользовательских интерфейсов на React",
                Requirements = "JavaScript, React, Redux, HTML5, CSS3",
                SalaryFrom = 120000,
                SalaryTo = 180000,
                EmploymentType = "full",
                WorkSchedule = "full_day",
                AuthorEmail = "ceo@startup.com"
            },
            new Vacancy
            {
                CompanyName = "GlobalSolutions",
                Title = "Java Backend Developer",
                Description = "Разработка backend систем на Java и Spring",
                Requirements = "Java, Spring Boot, Hibernate, PostgreSQL",
                SalaryFrom = 180000,
                SalaryTo = 300000,
                EmploymentType = "project",
                WorkSchedule = "flexible",
                AuthorEmail = "recruiter@global.org"
            },
            new Vacancy
            {
                CompanyName = "SoftDev Studio",
                Title = "Python Data Scientist",
                Description = "Анализ данных и машинное обучение на Python",
                Requirements = "Python, Pandas, NumPy, Scikit-learn, SQL",
                SalaryFrom = 200000,
                SalaryTo = 350000,
                EmploymentType = "full",
                WorkSchedule = "remote",
                AuthorEmail = "hr@softdev.com"
            },
            new Vacancy
            {
                CompanyName = "DataAnalytics Pro",
                Title = "Junior QA Engineer",
                Description = "Тестирование программного обеспечения",
                Requirements = "Основы тестирования, SQL, Postman",
                SalaryFrom = 80000,
                SalaryTo = 120000,
                EmploymentType = "internship",
                WorkSchedule = "full_day",
                AuthorEmail = "qa@dataanalytics.com"
            },
            new Vacancy
            {
                CompanyName = "MobileFirst",
                Title = "iOS Swift Developer",
                Description = "Разработка мобильных приложений для iOS",
                Requirements = "Swift, iOS SDK, UIKit, Core Data",
                SalaryFrom = 160000,
                SalaryTo = 220000,
                EmploymentType = "part",
                WorkSchedule = "remote",
                AuthorEmail = "dev@mobilefirst.com"
            }
        };

        public VacancySearchServiceTests()
        {
            _mockVacancyRepository = new Mock<IVacancyRepository>();
            _mockLogger = new Mock<ILogger<VacancySearchService>>();

            // Настраиваем mock репозитория возвращать тестовые вакансии
            _mockVacancyRepository.Setup(x => x.GetPublishedVacancies())
                .Returns(_testVacancies);

            _vacancySearchService = new VacancySearchService(
                _mockVacancyRepository.Object,
                _mockLogger.Object);
        }

        private SearchVacancyViewModel CreateValidSearchModel()
        {
            return new SearchVacancyViewModel
            {
                Keywords = "",
                CompanyName = "",
                SalaryFrom = null,
                SalaryTo = null,
                EmploymentType = "",
                WorkSchedule = "",
                Page = 1,
                PageSize = 10
            };
        }
    }
}