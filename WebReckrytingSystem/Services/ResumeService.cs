

// Services/ResumeService.cs
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
            // Проверка, что у пользователя еще нет резюме
            var existingResume = _resumeRepository.GetByUserEmail(userEmail);
            if (existingResume != null)
                return ServiceResult<Resume>.Error("У вас уже есть созданное резюме");

            // Валидация
            var validationResult = ValidateResume(model);
            if (!validationResult.IsSuccess)
                return ServiceResult<Resume>.Error(validationResult.Message);

            try
            {
                var resume = new Resume
                {
                    UserEmail = userEmail,
                    DesiredPosition = model.DesiredPosition.Trim(),
                    SalaryExpectations = model.SalaryExpectations,
                    ExperienceDescription = FormatExperienceDescription(model),
                    EducationDescription = FormatEducationDescription(model),
                    Skills = FormatSkills(model.Skills),
                    IsPublished = model.IsPublished
                };

                var savedResume = _resumeRepository.Save(resume);
                return ServiceResult<Resume>.Success("Резюме успешно создано!", savedResume);
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
                existingResume.SalaryExpectations = model.SalaryExpectations;
                existingResume.ExperienceDescription = FormatExperienceDescription(model);
                existingResume.EducationDescription = FormatEducationDescription(model);
                existingResume.Skills = FormatSkills(model.Skills);
                existingResume.IsPublished = model.IsPublished;

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

            if (model.Skills.Count > 20)
                return ServiceResult.Error("Слишком много навыков (максимум 20)");

            if (model.Skills.Any(string.IsNullOrWhiteSpace))
                return ServiceResult.Error("Навык не может быть пустым");

            // Проверка на дубликаты навыков
            var distinctSkills = model.Skills.Select(s => s.Trim().ToLower()).Distinct();
            if (distinctSkills.Count() != model.Skills.Count)
                return ServiceResult.Error("Обнаружены дублирующиеся навыки");

            return ServiceResult.Success("Валидация пройдена");
        }

        private string? FormatExperienceDescription(CreateResumeViewModel model)
        {
            var parts = new List<string>();

            if (model.ExperienceYears.HasValue)
                parts.Add($"Опыт работы: {model.ExperienceYears} лет");

            if (!string.IsNullOrWhiteSpace(model.ExperienceDescription))
                parts.Add(model.ExperienceDescription.Trim());

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

        public Resume? GetUserResume(string userEmail)
        {
            return _resumeRepository.GetByUserEmail(userEmail);
        }
    }
}