using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    public class EditResumeModel : PageModel
    {
        private readonly IResumeService _resumeService;

        public EditResumeModel(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [BindProperty]
        public EditResumeViewModel ResumeData { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Проверяем, что пользователь - соискатель
            if (User.FindFirst(ClaimTypes.Role)?.Value != "job_seeker")
            {
                return RedirectToPage("/AccessDenied");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Загружаем существующее резюме
            var existingResume = _resumeService.GetUserResume(userEmail);
            if (existingResume == null)
            {
                return RedirectToPage("/Account/CreateResume");
            }

            // Заполняем форму данными из существующего резюме
            FillFormFromResume(existingResume);

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                ErrorMessage = "Ошибка аутентификации";
                return Page();
            }

            // Используем CreateResumeViewModel для обновления (они идентичны)
            var updateModel = new CreateResumeViewModel
            {
                DesiredPosition = ResumeData.DesiredPosition,
                SalaryExpectations = ResumeData.SalaryExpectations,
                ExperienceYears = ResumeData.ExperienceYears,
                ExperienceDescription = ResumeData.ExperienceDescription,
                EducationalInstitution = ResumeData.EducationalInstitution,
                Faculty = ResumeData.Faculty,
                Specialization = ResumeData.Specialization,
                GraduationYear = ResumeData.GraduationYear,
                Skills = ResumeData.Skills,
                IsPublished = ResumeData.IsPublished
            };

            var result = _resumeService.UpdateResume(userEmail, updateModel);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Резюме успешно обновлено!";
                return RedirectToPage("/Account/ViewResume");
            }
            else
            {
                ErrorMessage = result.Message;
                return Page();
            }
        }

        private void FillFormFromResume(Resume resume)
        {
            ResumeData.DesiredPosition = resume.DesiredPosition;
            ResumeData.SalaryExpectations = resume.SalaryExpectations;
            ResumeData.IsPublished = resume.IsPublished;

            // Парсим описание опыта для извлечения лет опыта
            ParseExperienceFromDescription(resume.ExperienceDescription);

            // Парсим образование
            ParseEducationFromDescription(resume.EducationDescription);

            // Парсим навыки
            ParseSkillsFromString(resume.Skills);
        }

        private void ParseExperienceFromDescription(string? experienceDescription)
        {
            if (string.IsNullOrEmpty(experienceDescription))
                return;

            // Пытаемся извлечь количество лет опыта из описания
            var yearsMatch = System.Text.RegularExpressions.Regex.Match(experienceDescription, @"Опыт работы:\s*(\d+)\s*лет");
            if (yearsMatch.Success && int.TryParse(yearsMatch.Groups[1].Value, out int years))
            {
                ResumeData.ExperienceYears = years;
            }

            // Убираем строку с опытом из описания
            ResumeData.ExperienceDescription = System.Text.RegularExpressions.Regex
                .Replace(experienceDescription, @"Опыт работы:\s*\d+\s*лет[\n\r]*", "")
                .Trim();
        }

        private void ParseEducationFromDescription(string? educationDescription)
        {
            if (string.IsNullOrEmpty(educationDescription))
                return;

            // Простой парсинг формата: "Университет, Факультет, Специализация, 2020 г."
            var parts = educationDescription.Split(',');
            if (parts.Length > 0)
            {
                ResumeData.EducationalInstitution = parts[0].Trim();

                if (parts.Length > 1)
                    ResumeData.Faculty = parts[1].Trim();

                if (parts.Length > 2)
                    ResumeData.Specialization = parts[2].Trim();

                if (parts.Length > 3)
                {
                    var yearMatch = System.Text.RegularExpressions.Regex.Match(parts[3], @"(\d{4})");
                    if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out int year))
                    {
                        ResumeData.GraduationYear = year;
                    }
                }
            }
        }

        private void ParseSkillsFromString(string? skills)
        {
            if (!string.IsNullOrEmpty(skills))
            {
                ResumeData.Skills = skills.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
        }
    }
}