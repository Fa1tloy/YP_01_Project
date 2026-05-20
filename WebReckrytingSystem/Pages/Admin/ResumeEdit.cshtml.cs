using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Models.Admin;
using Microsoft.Extensions.Logging;

namespace WebReckrytingSystem.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class ResumeEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResumeEditModel> _logger;

        public ResumeEditModel(ApplicationDbContext context, ILogger<ResumeEditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public AdminResumeViewModel ResumeData { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string email)
        {
            _logger.LogInformation("=== OnGetAsync для {Email} ===", email);

            var resume = await _context.Resumes.Include(r => r.User).FirstOrDefaultAsync(r => r.UserEmail == email);
            if (resume == null)
            {
                _logger.LogWarning("Резюме не найдено");
                return NotFound();
            }

            ResumeData = new AdminResumeViewModel
            {
                DesiredPosition = resume.DesiredPosition,
                SalaryExpectations = resume.SalaryExpectations,
                City = resume.City,
                BusinessTripReadiness = resume.BusinessTripReadiness,
                SearchStatus = resume.SearchStatus,
                Age = resume.Age,
                EmploymentType = resume.EmploymentType,
                WorkSchedule = resume.WorkSchedule,
                Specialty = resume.Specialty,
                Gender = resume.Gender,
                HasCar = resume.HasCar,
                DriverLicenseCategory = resume.DriverLicenseCategory,
                ExperienceDescription = resume.ExperienceDescription,
                EducationDescription = resume.EducationDescription,
                Skills = resume.Skills,
                IsPublished = resume.IsPublished
            };

            if (!string.IsNullOrEmpty(resume.PracticesJson))
            {
                ResumeData.Practices = JsonSerializer.Deserialize<List<PracticeViewModel>>(resume.PracticesJson) ?? new();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string email)
        {
            _logger.LogInformation("=== OnPostAsync для {Email} ===", email);

            var resume = await _context.Resumes.FindAsync(email);
            if (resume == null)
            {
                _logger.LogError("Резюме не найдено в POST");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState не валиден");
                return Page();
            }

            try
            {
                resume.DesiredPosition = ResumeData.DesiredPosition.Trim();
                resume.SalaryExpectations = ResumeData.SalaryExpectations;
                resume.City = ResumeData.City.Trim();
                resume.BusinessTripReadiness = ResumeData.BusinessTripReadiness.Trim();
                resume.SearchStatus = ResumeData.SearchStatus.Trim();
                resume.Age = ResumeData.Age;
                resume.EmploymentType = ResumeData.EmploymentType.Trim();
                resume.WorkSchedule = ResumeData.WorkSchedule.Trim();
                resume.Specialty = ResumeData.Specialty.Trim();
                resume.Gender = ResumeData.Gender.Trim();
                resume.HasCar = ResumeData.HasCar;
                resume.DriverLicenseCategory = ResumeData.DriverLicenseCategory?.Trim();
                resume.ExperienceDescription = ResumeData.ExperienceDescription?.Trim();
                resume.EducationDescription = ResumeData.EducationDescription?.Trim();
                resume.Skills = ResumeData.Skills?.Trim();
                resume.IsPublished = ResumeData.IsPublished;
                resume.PracticesJson = JsonSerializer.Serialize(ResumeData.Practices);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Резюме успешно обновлено!";
                return RedirectToPage("/Admin/Resumes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении резюме");
                ErrorMessage = $"Ошибка при сохранении: {ex.Message}";
                return Page();
            }
        }
    }
}
