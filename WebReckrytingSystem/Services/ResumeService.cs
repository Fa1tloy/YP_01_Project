using System.Net.NetworkInformation;
using System.Text.Json;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IResumeRepository _resumeRepository;

        public ResumeService(IResumeRepository resumeRepository)
        {
            _resumeRepository = resumeRepository;
        }

        public ServiceResult<Resume> CreateResume(string userEmail, CreateResumeViewModel model)
        {
            var existingResume = _resumeRepository.GetByUserEmail(userEmail);
            if (existingResume != null)
                return ServiceResult<Resume>.Error("У вас уже есть созданное резюме");

            var validationResult = ValidateResume(model);
            if (!validationResult.IsSuccess)
                return ServiceResult<Resume>.Error(validationResult.Message);

            try
            {
                var resume = new Resume
                {
                    UserEmail = userEmail,
                    DesiredPosition = model.DesiredPosition.Trim(),
                    City = model.City.Trim(),
                    BusinessTripReadiness = model.BusinessTripReadiness,
                    SearchStatus = model.SearchStatus,
                    Age = model.Age,
                    EmploymentType = model.EmploymentType,
                    WorkSchedule = model.WorkSchedule,
                    Specialty = model.Specialty.Trim(),
                    Gender = model.Gender,
                    SalaryExpectations = model.SalaryExpectations,
                    HasCar = model.HasCar,
                    DriverLicenseCategory = FormatDriverLicenseCategories(model.DriverLicenseCategories),
                    ExperienceDescription = FormatExperienceDescription(model),
                    EducationDescription = FormatEducationDescription(model),
                    Skills = string.Join(", ", model.Skills.Select(s => s.Trim()).Distinct()),
                    IsPublished = true,
                    PracticesJson = JsonSerializer.Serialize(model.Practices)
                };

                var savedResume = _resumeRepository.Save(resume);
                return ServiceResult<Resume>.Success("Резюме успешно создано и опубликовано!", savedResume);
            }
            catch (Exception ex)
            {
                return ServiceResult<Resume>.Error($"Ошибка при создании резюме: {ex.Message}");
            }
        }

        public ServiceResult<Resume> UpdateResume(string userEmail, CreateResumeViewModel model)
        {
            var validationResult = ValidateResume(model);
            if (!validationResult.IsSuccess)
                return ServiceResult<Resume>.Error(validationResult.Message);

            var existingResume = _resumeRepository.GetByUserEmail(userEmail);
            if (existingResume == null)
                return ServiceResult<Resume>.Error("Резюме не найдено");

            try
            {
                existingResume.DesiredPosition = model.DesiredPosition.Trim();
                existingResume.City = model.City.Trim();
                existingResume.BusinessTripReadiness = model.BusinessTripReadiness;
                existingResume.SearchStatus = model.SearchStatus;
                existingResume.Age = model.Age;
                existingResume.EmploymentType = model.EmploymentType;
                existingResume.WorkSchedule = model.WorkSchedule;
                existingResume.Specialty = model.Specialty.Trim();
                existingResume.Gender = model.Gender;
                existingResume.SalaryExpectations = model.SalaryExpectations;
                existingResume.HasCar = model.HasCar;
                existingResume.DriverLicenseCategory = FormatDriverLicenseCategories(model.DriverLicenseCategories);
                existingResume.ExperienceDescription = FormatExperienceDescription(model);
                existingResume.EducationDescription = FormatEducationDescription(model);
                existingResume.Skills = string.Join(", ", model.Skills.Select(s => s.Trim()).Distinct());
                existingResume.IsPublished = model.IsPublished;
                existingResume.PracticesJson = JsonSerializer.Serialize(model.Practices);

                var updatedResume = _resumeRepository.Update(existingResume);
                return ServiceResult<Resume>.Success("Резюме успешно обновлено!", updatedResume);
            }
            catch (Exception ex)
            {
                return ServiceResult<Resume>.Error($"Ошибка при обновлении резюме: {ex.Message}");
            }
        }

        private ServiceResult ValidateResume(CreateResumeViewModel model)
        {
            if (model.GraduationYear > DateTime.Now.Year)
                return ServiceResult.Error("Год окончания не может быть в будущем");

            if (!model.Age.HasValue)
                return ServiceResult.Error("Возраст обязателен");

            if (model.Skills.Count > 20)
                return ServiceResult.Error("Слишком много навыков (максимум 20)");

            if (model.Skills.Any(string.IsNullOrWhiteSpace))
                return ServiceResult.Error("Навык не может быть пустым");

            if (!SpecialtyCatalog.All.Contains(model.Specialty))
                return ServiceResult.Error("Выберите специальность из списка");

            if (model.DriverLicenseCategories.Any(c => !DriverLicenseCategoryCatalog.All.Contains(c)))
                return ServiceResult.Error("Выберите категории прав из списка");

            var distinctSkills = model.Skills.Select(s => s.Trim().ToLower()).Distinct();
            if (distinctSkills.Count() != model.Skills.Count)
                return ServiceResult.Error("Обнаружены дублирующиеся навыки");

            if (model.Practices != null && model.Practices.Any())
            {
                foreach (var p in model.Practices)
                {
                    if (!p.IsValid)
                        return ServiceResult.Error("Дата окончания практики не может быть раньше даты начала");
                }
            }

            return ServiceResult.Success("Валидация пройдена");
        }

        private string? FormatExperienceDescription(CreateResumeViewModel model)
        {
            var parts = new List<string>();

            if (model.ExperienceYears.HasValue)
                parts.Add($"Опыт работы: {model.ExperienceYears} лет");

            if (!string.IsNullOrWhiteSpace(model.ExperienceDescription))
                parts.Add(model.ExperienceDescription.Trim());

            var practicesText = FormatPractices(model.Practices);
            if (practicesText != null)
                parts.Add(practicesText);

            return parts.Any() ? string.Join("\n", parts) : null;
        }

        private string? FormatEducationDescription(CreateResumeViewModel model)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.EducationalInstitution))
            {
                var education = model.EducationalInstitution.Trim();

                if (!string.IsNullOrWhiteSpace(model.Faculty))
                    education += $", {model.Faculty.Trim()}";

                if (!string.IsNullOrWhiteSpace(model.Specialization))
                    education += $", {model.Specialization.Trim()}";

                if (model.GraduationYear.HasValue)
                    education += $", {model.GraduationYear} г.";

                parts.Add(education);
            }

            return parts.Any() ? string.Join("; ", parts) : null;
        }

        private string? FormatSkills(List<string> skills)
        {
            var validSkills = skills.Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Select(s => s.Trim())
                                   .Distinct();
            return validSkills.Any() ? string.Join(", ", validSkills) : null;
        }

        private string? FormatDriverLicenseCategories(List<string> categories)
        {
            var validCategories = categories
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            return validCategories.Any() ? string.Join(", ", validCategories) : null;
        }

        private string? FormatPractices(List<PracticeViewModel> practices)
        {
            if (practices == null || !practices.Any()) return null;

            var parts = new List<string> { "Практики:" };
            foreach (var p in practices)
            {
                var period = $"{p.StartDate:MM.yyyy} – {p.EndDate:MM.yyyy}";
                parts.Add($"• {p.Place} ({period}){(string.IsNullOrWhiteSpace(p.Description) ? "" : $" — {p.Description}")}");
            }
            return string.Join("\n", parts);
        }

        public Resume? GetUserResume(string userEmail)
        {
            return _resumeRepository.GetByUserEmail(userEmail);
        }
    }
}
