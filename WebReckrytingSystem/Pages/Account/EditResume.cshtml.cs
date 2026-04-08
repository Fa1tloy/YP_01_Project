using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        public CreateResumeViewModel ResumeData { get; set; } = new();
        public IReadOnlyList<string> Specialties => SpecialtyCatalog.All;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.FindFirst(ClaimTypes.Role)?.Value != "job_seeker")
            {
                return RedirectToPage("/AccessDenied");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            var existingResume = _resumeService.GetUserResume(userEmail);
            if (existingResume == null)
            {
                TempData["ErrorMessage"] = "Сначала создайте резюме";
                return RedirectToPage("/Account/CreateResume");
            }

            ResumeData = MapResumeToViewModel(existingResume);
            return Page();
        }

        public IActionResult OnPost()
        {
            var skillsHidden = Request.Form["SkillsHidden"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(skillsHidden))
            {
                try
                {
                    ResumeData.Skills = JsonSerializer.Deserialize<List<string>>(skillsHidden) ?? new List<string>();
                }
                catch
                {
                    ResumeData.Skills = new List<string>();
                }
            }

            ModelState.Remove("ResumeData.Skills");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                ModelState.AddModelError(string.Empty, "Ошибка аутентификации");
                return Page();
            }

            var result = _resumeService.UpdateResume(userEmail, ResumeData);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Не удалось обновить резюме");
                return Page();
            }

            TempData["SuccessMessage"] = "Резюме успешно обновлено!";
            return RedirectToPage("/Account/ViewResume");
        }

        private static CreateResumeViewModel MapResumeToViewModel(Models.Resume resume)
        {
            var viewModel = new CreateResumeViewModel
            {
                DesiredPosition = resume.DesiredPosition,
                City = resume.City,
                BusinessTripReadiness = resume.BusinessTripReadiness,
                SearchStatus = resume.SearchStatus,
                Age = resume.Age,
                EmploymentType = resume.EmploymentType,
                WorkSchedule = resume.WorkSchedule,
                Specialty = resume.Specialty,
                Gender = resume.Gender,
                SalaryExpectations = resume.SalaryExpectations,
                HasCar = resume.HasCar,
                DriverLicenseCategory = resume.DriverLicenseCategory,
                IsPublished = resume.IsPublished,
                Skills = string.IsNullOrWhiteSpace(resume.Skills)
                    ? new List<string>()
                    : resume.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            };

            MapEducation(viewModel, resume.EducationDescription);
            MapExperience(viewModel, resume.ExperienceDescription);
            MapPractices(viewModel, resume.PracticesJson);

            return viewModel;
        }

        private static void MapEducation(CreateResumeViewModel viewModel, string? educationDescription)
        {
            if (string.IsNullOrWhiteSpace(educationDescription))
            {
                return;
            }

            var parts = educationDescription
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (parts.Count > 0)
            {
                viewModel.EducationalInstitution = parts[0];
            }

            if (parts.Count > 1)
            {
                viewModel.Faculty = parts[1];
            }

            if (parts.Count > 2)
            {
                var yearPart = parts.FirstOrDefault(p => Regex.IsMatch(p, @"\b\d{4}\b"));
                if (yearPart != null)
                {
                    var yearMatch = Regex.Match(yearPart, @"\b\d{4}\b");
                    if (yearMatch.Success && int.TryParse(yearMatch.Value, out var year))
                    {
                        viewModel.GraduationYear = year;
                    }

                    var specializationParts = parts.Where(p => p != yearPart).Skip(2);
                    viewModel.Specialization = specializationParts.Any() ? string.Join(", ", specializationParts) : null;
                }
                else
                {
                    viewModel.Specialization = string.Join(", ", parts.Skip(2));
                }
            }

            if (string.IsNullOrWhiteSpace(viewModel.EducationalInstitution))
            {
                viewModel.EducationalInstitution = educationDescription;
            }
        }

        private static void MapExperience(CreateResumeViewModel viewModel, string? experienceDescription)
        {
            if (string.IsNullOrWhiteSpace(experienceDescription))
            {
                return;
            }

            var lines = experienceDescription
                .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (lines.Count == 0)
            {
                return;
            }

            var yearsMatch = Regex.Match(lines[0], @"^Опыт работы:\s*(\d+)\s*лет", RegexOptions.IgnoreCase);
            if (yearsMatch.Success && int.TryParse(yearsMatch.Groups[1].Value, out var years))
            {
                viewModel.ExperienceYears = years;
                lines.RemoveAt(0);
            }

            var practicesStartIndex = lines.FindIndex(line => line.StartsWith("Практики:", StringComparison.OrdinalIgnoreCase));
            if (practicesStartIndex >= 0)
            {
                lines = lines.Take(practicesStartIndex).ToList();
            }

            if (lines.Any())
            {
                viewModel.ExperienceDescription = string.Join(Environment.NewLine, lines);
            }
        }

        private static void MapPractices(CreateResumeViewModel viewModel, string? practicesJson)
        {
            if (string.IsNullOrWhiteSpace(practicesJson))
            {
                return;
            }

            try
            {
                viewModel.Practices = JsonSerializer.Deserialize<List<PracticeViewModel>>(practicesJson) ?? new List<PracticeViewModel>();
            }
            catch
            {
                viewModel.Practices = new List<PracticeViewModel>();
            }
        }
    }
}
