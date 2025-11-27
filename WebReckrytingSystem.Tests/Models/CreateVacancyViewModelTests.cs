using System.ComponentModel.DataAnnotations;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Tests.Models
{
    [TestClass]
    public class CreateVacancyViewModelTests
    {
        [TestMethod]
        public void CreateVacancyViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = "Senior .NET Developer",
                Description = "Разработка высоконагруженных приложений",
                Requirements = "Опыт работы 3+ года, знание C#, ASP.NET",
                SalaryFrom = 150000,
                SalaryTo = 250000,
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(model, context, results, true);

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void CreateVacancyViewModel_EmptyTitle_FailsValidation()
        {
            // Arrange
            var model = new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = "", // Пустое название
                Description = "Описание",
                Requirements = "Требования",
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(model, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Title")));
        }

        [TestMethod]
        public void CreateVacancyViewModel_EmptyDescription_FailsValidation()
        {
            // Arrange
            var model = new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = "Senior .NET Developer",
                Description = "", // Пустое описание
                Requirements = "Требования",
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(model, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Description")));
        }

        [TestMethod]
        public void CreateVacancyViewModel_SalaryOutOfRange_FailsValidation()
        {
            // Arrange
            var model = new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = "Senior .NET Developer",
                Description = "Описание",
                Requirements = "Требования",
                SalaryFrom = 10000000, // Превышает максимальное значение
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(model, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("SalaryFrom")));
        }

        [TestMethod]
        public void CreateVacancyViewModel_TitleTooLong_FailsValidation()
        {
            // Arrange
            var model = new CreateVacancyViewModel
            {
                CompanyName = "TechCorp",
                Title = new string('A', 256), // Превышает 255 символов
                Description = "Описание",
                Requirements = "Требования",
                EmploymentType = "full",
                WorkSchedule = "full_day"
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(model, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Title")));
        }
    }
}