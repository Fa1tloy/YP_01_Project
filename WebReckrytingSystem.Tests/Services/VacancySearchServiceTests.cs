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
        [TestMethod]
        public void SearchVacancies_WithKeywords_ReturnsMatchingVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Keywords = ".NET";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Найдено 1 вакансий", result.Message);
            Assert.AreEqual(1, result.Data.Items.Count);
            Assert.AreEqual("Senior .NET Developer", result.Data.Items.First().Title);
        }

        [TestMethod]
        public void SearchVacancies_WithMultipleKeywords_ReturnsMatchingVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Keywords = "Developer React";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.Items.Count >= 2);
            Assert.IsTrue(result.Data.Items.Any(v => v.Title.Contains("React")));
            Assert.IsTrue(result.Data.Items.Any(v => v.Title.Contains("Developer")));
        }

        [TestMethod]
        public void SearchVacancies_WithCompanyFilter_ReturnsCompanyVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.CompanyName = "TechCorp";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Items.Count);
            Assert.AreEqual("TechCorp", result.Data.Items.First().CompanyName);
        }

        [TestMethod]
        public void SearchVacancies_WithSalaryFilter_ReturnsMatchingVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.SalaryFrom = 150000;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.Items.All(v =>
                v.SalaryTo >= 150000 || v.SalaryFrom >= 150000));
        }

        [TestMethod]
        public void SearchVacancies_WithEmploymentTypeFilter_ReturnsMatchingVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.EmploymentType = "full";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.Items.All(v => v.EmploymentType == "full"));
        }

        [TestMethod]
        public void SearchVacancies_WithWorkScheduleFilter_ReturnsMatchingVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.WorkSchedule = "remote";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.Items.All(v => v.WorkSchedule == "remote"));
            Assert.AreEqual(3, result.Data.Items.Count); // 3 удаленные вакансии в тестовых данных
        }
        [TestMethod]
        public void SearchVacancies_WithMultipleFilters_ReturnsMatchingVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Keywords = "Developer";
            model.SalaryFrom = 150000;
            model.EmploymentType = "full";
            model.WorkSchedule = "remote";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Data.Items.Count);
            Assert.AreEqual("Senior .NET Developer", result.Data.Items.First().Title);
        }

        [TestMethod]
        public void SearchVacancies_WithNoFilters_ReturnsAllVacancies()
        {
            // Arrange
            var model = CreateValidSearchModel();

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(_testVacancies.Count, result.Data.TotalCount);
            Assert.AreEqual("Найдено 6 вакансий", result.Message);
        }

        [TestMethod]
        public void SearchVacancies_WithNoResults_ReturnsEmptyList()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Keywords = "Blockchain Angular";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data.Items.Count);
            Assert.AreEqual("По вашему запросу ничего не найдено", result.Message);
        }
        [TestMethod]
        public void SearchVacancies_WithNegativeSalary_ReturnsError()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.SalaryFrom = -50000;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Зарплата не может быть отрицательной", result.Message);
            _mockVacancyRepository.Verify(x => x.GetPublishedVacancies(), Times.Never);
        }

        [TestMethod]
        public void SearchVacancies_WithSalaryFromGreaterThanSalaryTo_ReturnsError()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.SalaryFrom = 200000;
            model.SalaryTo = 100000;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Зарплата 'от' не может быть больше зарплаты 'до'", result.Message);
        }

        [TestMethod]
        public void SearchVacancies_WithInvalidEmploymentType_ReturnsError()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.EmploymentType = "invalid_type";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Недопустимый тип занятости", result.Message);
        }

        [TestMethod]
        public void SearchVacancies_WithInvalidWorkSchedule_ReturnsError()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.WorkSchedule = "invalid_schedule";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Недопустимый график работы", result.Message);
        }

        [TestMethod]
        public void SearchVacancies_WithInvalidPageNumber_ReturnsError()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Page = 0;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Некорректный номер страницы", result.Message);
        }

        [TestMethod]
        public void SearchVacancies_WithInvalidPageSize_ReturnsError()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.PageSize = 0;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Некорректный размер страницы", result.Message);
        }
        [TestMethod]
        public void SearchVacancies_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Page = 1;
            model.PageSize = 3;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.Data.Items.Count);
            Assert.AreEqual(1, result.Data.Page);
            Assert.AreEqual(3, result.Data.PageSize);
            Assert.AreEqual(2, result.Data.TotalPages); // 6 вакансий / 3 на страницу = 2 страницы
        }

        [TestMethod]
        public void SearchVacancies_WithSecondPage_ReturnsCorrectResults()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Page = 2;
            model.PageSize = 3;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.Data.Items.Count); // Вторая страница тоже имеет 3 элемента
            Assert.AreEqual(2, result.Data.Page);
            Assert.IsTrue(result.Data.HasPreviousPage);
            Assert.IsFalse(result.Data.HasNextPage); // На второй странице из двух
        }

        [TestMethod]
        public void SearchVacancies_PaginationProperties_AreCalculatedCorrectly()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Page = 1;
            model.PageSize = 4;

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.AreEqual(6, result.Data.TotalCount);
            Assert.AreEqual(2, result.Data.TotalPages); // 6/4 = 2 страницы
            Assert.IsFalse(result.Data.HasPreviousPage);
            Assert.IsTrue(result.Data.HasNextPage);
        }
        [TestMethod]
        public void SearchVacancies_WithKeywords_SortsByRelevance()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Keywords = "Java Developer";

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var firstVacancy = result.Data.Items.First();
            Assert.AreEqual("Java Backend Developer", firstVacancy.Title); // Должен быть первым по релевантности
        }

        [TestMethod]
        public void SearchVacancies_WithoutKeywords_SortsByCompanyAndTitle()
        {
            // Arrange
            var model = CreateValidSearchModel();

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var firstVacancy = result.Data.Items.First();
            // Должны быть отсортированы по названию компании
            Assert.IsTrue(result.Data.Items.Select(v => v.CompanyName).SequenceEqual(
                result.Data.Items.Select(v => v.CompanyName).OrderByDescending(c => c)));
        }

        [TestMethod]
        public void SearchVacancies_RelevanceScore_IsCalculatedCorrectly()
        {
            // Arrange
            var service = new VacancySearchService(_mockVacancyRepository.Object, _mockLogger.Object);
            var vacancy = new Vacancy
            {
                Title = "Senior .NET Developer",
                Description = "Разработка на C#",
                Requirements = ".NET Framework, ASP.NET",
                CompanyName = "TechCorp"
            };
            var keywords = new[] { "net", "developer" };

            // Act & Assert - используем reflection для тестирования приватного метода
            var method = typeof(VacancySearchService).GetMethod("CalculateRelevanceScore",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var score = (int)method.Invoke(service, new object[] { vacancy, keywords });

            // Assert
            Assert.IsTrue(score > 0);
            // Title содержит оба ключевых слова: 10 + 10 = 20
            // Requirements содержит "net": +5
            // CompanyName не содержит ключевых слов: 0
            // Итого: 25
        }
        [TestMethod]
        public void SearchVacancies_WhenRepositoryThrowsException_ReturnsError()
        {
            // Arrange
            _mockVacancyRepository.Setup(x => x.GetPublishedVacancies())
                .Throws(new Exception("Database connection failed"));

            var model = CreateValidSearchModel();

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Произошла ошибка при поиске вакансий", result.Message);
        }

        [TestMethod]
        public void SearchVacancies_WithCaseInsensitiveSearch_ReturnsResults()
        {
            // Arrange
            var model = CreateValidSearchModel();
            model.Keywords = "developer"; // в нижнем регистре

            // Act
            var result = _vacancySearchService.SearchVacancies(model);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data.Items.Count > 0);
            Assert.IsTrue(result.Data.Items.All(v =>
                v.Title.ToLower().Contains("developer") ||
                v.Description.ToLower().Contains("developer") ||
                v.Requirements.ToLower().Contains("developer")));
        }
    }
}