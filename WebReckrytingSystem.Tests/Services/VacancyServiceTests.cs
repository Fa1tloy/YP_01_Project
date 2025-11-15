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
        [TestMethod]
        public void CreateVacancy_ByJobSeeker_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            _mockUserRepository.Setup(x => x.FindByEmail(_jobSeekerUser.Email))
                .Returns(_jobSeekerUser);

            // Act
            var result = _vacancyService.CreateVacancy(_jobSeekerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Только работодатели могут создавать вакансии", result.Message);
            _mockVacancyRepository.Verify(x => x.Save(It.IsAny<Vacancy>()), Times.Never);
        }

        [TestMethod]
        public void CreateVacancy_WithNonExistentUser_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            _mockUserRepository.Setup(x => x.FindByEmail("nonexistent@example.com"))
                .Returns((User)null);

            // Act
            var result = _vacancyService.CreateVacancy("nonexistent@example.com", model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Только работодатели могут создавать вакансии", result.Message);
        }
        [TestMethod]
        public void CreateVacancy_WithNonExistentCompany_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName("UnknownCompany"))
                .Returns((Company)null);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Компания не найдена", result.Message);
        }

        [TestMethod]
        public void CreateVacancy_WithEmptyTitle_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            model.Title = "";

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Название вакансии обязательно", result.Message);
        }
        [TestMethod]
        public void CreateVacancy_WithEmptyDescription_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            model.Description = "";

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Описание вакансии обязательно", result.Message);
        }

        [TestMethod]
        public void CreateVacancy_WithEmptyRequirements_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            model.Requirements = "";

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Требования к кандидату обязательны", result.Message);
        }
        [TestMethod]
        public void CreateVacancy_WithSalaryFromGreaterThanSalaryTo_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            model.SalaryFrom = 300000;
            model.SalaryTo = 200000;

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Зарплата 'от' не может быть больше зарплаты 'до'", result.Message);
        }

        [TestMethod]
        public void CreateVacancy_WithNegativeSalary_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            model.SalaryFrom = -50000;
            model.SalaryTo = 100000;

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Зарплата не может быть отрицательной", result.Message);
        }
        [TestMethod]
        public void CreateVacancy_WithDuplicateTitleInCompany_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            var existingVacancy = new Vacancy
            {
                CompanyName = model.CompanyName,
                Title = model.Title,
                AuthorEmail = "other@company.com"
            };

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);
            _mockVacancyRepository.Setup(x => x.GetByCompanyAndTitle(model.CompanyName, model.Title))
                .Returns(existingVacancy);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Вакансия с таким названием уже существует в этой компании", result.Message);
            _mockVacancyRepository.Verify(x => x.Save(It.IsAny<Vacancy>()), Times.Never);
        }
        [TestMethod]
        public void CreateVacancy_WithInvalidEmploymentType_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            model.EmploymentType = "invalid_type";

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Недопустимый тип занятости", result.Message);
        }

        [TestMethod]
        public void CreateVacancy_WithInvalidWorkSchedule_ReturnsError()
        {
            // Arrange
            var model = CreateValidVacancyModel();
            model.WorkSchedule = "invalid_schedule";

            _mockUserRepository.Setup(x => x.FindByEmail(_employerUser.Email))
                .Returns(_employerUser);
            _mockCompanyRepository.Setup(x => x.FindByName(model.CompanyName))
                .Returns(_testCompany);

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Недопустимый график работы", result.Message);
        }

        [TestMethod]
        public void CreateVacancy_WithValidEmploymentTypes_ReturnsSuccess()
        {
            // Arrange
            var validTypes = new[] { "full", "part", "project", "internship", "volunteer" };

            foreach (var employmentType in validTypes)
            {
                var model = CreateValidVacancyModel();
                model.EmploymentType = employmentType;

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
                Assert.IsTrue(result.IsSuccess, $"Должен принимать тип занятости: {employmentType}");
            }
        }
        [TestMethod]
        public void CreateVacancy_WhenRepositoryThrowsException_ReturnsError()
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
                .Throws(new Exception("Database connection failed"));

            // Act
            var result = _vacancyService.CreateVacancy(_employerUser.Email, model);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message.Contains("Ошибка при создании вакансии"));
        }
        [TestMethod]
        public void GetUserVacancies_ReturnsUserVacancies()
        {
            // Arrange
            var userVacancies = new List<Vacancy>
            {
                new Vacancy { CompanyName = "TechCorp", Title = "Developer 1", AuthorEmail = _employerUser.Email },
                new Vacancy { CompanyName = "TechCorp", Title = "Developer 2", AuthorEmail = _employerUser.Email }
            };

            _mockVacancyRepository.Setup(x => x.GetByAuthor(_employerUser.Email))
                .Returns(userVacancies);

            // Act
            var result = _vacancyService.GetUserVacancies(_employerUser.Email);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(v => v.AuthorEmail == _employerUser.Email));
        }

        [TestMethod]
        public void GetVacancy_WithExistingVacancy_ReturnsVacancy()
        {
            // Arrange
            var expectedVacancy = new Vacancy
            {
                CompanyName = "TechCorp",
                Title = "Senior .NET Developer",
                AuthorEmail = _employerUser.Email
            };

            _mockVacancyRepository.Setup(x => x.GetByCompanyAndTitle("TechCorp", "Senior .NET Developer"))
                .Returns(expectedVacancy);

            // Act
            var result = _vacancyService.GetVacancy("TechCorp", "Senior .NET Developer");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedVacancy.Title, result.Title);
            Assert.AreEqual(expectedVacancy.CompanyName, result.CompanyName);
        }

        [TestMethod]
        public void GetVacancy_WithNonExistingVacancy_ReturnsNull()
        {
            // Arrange
            _mockVacancyRepository.Setup(x => x.GetByCompanyAndTitle("Unknown", "Vacancy"))
                .Returns((Vacancy)null);

            // Act
            var result = _vacancyService.GetVacancy("Unknown", "Vacancy");

            // Assert
            Assert.IsNull(result);
        }
    }
}

