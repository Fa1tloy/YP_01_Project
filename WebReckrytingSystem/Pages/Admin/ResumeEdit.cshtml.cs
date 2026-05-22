using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class ResumeEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpecialtyService _specialtyService;

        public ResumeEditModel(ApplicationDbContext context, ISpecialtyService specialtyService)
        {
            _context = context;
            _specialtyService = specialtyService;
        }

        [BindProperty]
        public CreateResumeViewModel ResumeData { get; set; } = new();

        public IReadOnlyList<string> Specialties { get; set; } = new List<string>();
        public IReadOnlyList<string> DriverLicenseCategories => DriverLicenseCategoryCatalog.All;
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(string email)
        {
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
            Specialties = _specialtyService.GetAllNames();

            var existingResume = _context.Resumes.FirstOrDefault(r => r.UserEmail == email);
            if (existingResume == null)
            {
                return NotFound();
            }

            ResumeData = MapResumeToViewModel(existingResume);
            return Page();
        }

        public IActionResult OnPost(string email)
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

            ResumeData.EducationalInstitution = "ФГБОУ Колледж Росрезерва";
            Specialties = _specialtyService.GetAllNames();
            ModelState.Remove("ResumeData.Skills");

            if (!ModelState.IsValid)
                return Page();

            var existingResume = _context.Resumes.FirstOrDefault(r => r.UserEmail == email);
            if (existingResume == null)
                return NotFound();

            existingResume.DesiredPosition = ResumeData.DesiredPosition.Trim();
            existingResume.City = ResumeData.City.Trim();
            existingResume.BusinessTripReadiness = ResumeData.BusinessTripReadiness;
            existingResume.SearchStatus = ResumeData.SearchStatus;
            existingResume.Age = ResumeData.Age;
            existingResume.EmploymentType = ResumeData.EmploymentType;
            existingResume.WorkSchedule = ResumeData.WorkSchedule;
            existingResume.Specialty = ResumeData.Specialty.Trim();
            existingResume.Gender = ResumeData.Gender;
            existingResume.SalaryExpectations = ResumeData.SalaryExpectations;
            existingResume.HasCar = ResumeData.HasCar;
            existingResume.DriverLicenseCategory = FormatDriverLicenseCategories(ResumeData.DriverLicenseCategories);
            existingResume.ExperienceDescription = FormatExperienceDescription(ResumeData);
            existingResume.EducationDescription = FormatEducationDescription(ResumeData);
            existingResume.Skills = string.Join(", ", ResumeData.Skills.Select(s => s.Trim()).Distinct());
            existingResume.IsPublished = ResumeData.IsPublished;
            existingResume.PracticesJson = JsonSerializer.Serialize(ResumeData.Practices);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Резюме успешно обновлено!";
            return RedirectToPage("/Admin/Resumes");
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
                DriverLicenseCategories = string.IsNullOrWhiteSpace(resume.DriverLicenseCategory)
                    ? new List<string>()
                    : resume.DriverLicenseCategory
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList(),
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

            var yearMatch = Regex.Match(educationDescription, @"\b\d{4}\b");
            if (yearMatch.Success && int.TryParse(yearMatch.Value, out var year))
            {
                viewModel.GraduationYear = year;
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

        private static string? FormatExperienceDescription(CreateResumeViewModel model)
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

        private static string? FormatEducationDescription(CreateResumeViewModel model)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.EducationalInstitution))
            {
                var education = model.EducationalInstitution.Trim();

                if (model.GraduationYear.HasValue)
                    education += $", {model.GraduationYear} г.";

                parts.Add(education);
            }

            return parts.Any() ? string.Join("; ", parts) : null;
        }

        private static string? FormatDriverLicenseCategories(List<string> categories)
        {
            var validCategories = categories
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            return validCategories.Any() ? string.Join(", ", validCategories) : null;
        }

        private static string? FormatPractices(List<PracticeViewModel> practices)
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
    }
}
