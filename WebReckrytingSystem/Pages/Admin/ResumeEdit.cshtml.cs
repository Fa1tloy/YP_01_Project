using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Models.Admin; // Добавьте это
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
        public AdminResumeViewModel ResumeData { get; set; } = new(); // Используем новую модель

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string email)
        {
            _logger.LogInformation($"=== OnGetAsync для {email} ===");

            var resume = await _context.Resumes.Include(r => r.User).FirstOrDefaultAsync(r => r.UserEmail == email);
            if (resume == null)
            {
                _logger.LogWarning("Резюме не найдено!");
                return NotFound();
            }

            // Простое присвоение без промежуточных полей
            ResumeData = new AdminResumeViewModel
            {
                DesiredPosition = resume.DesiredPosition,
                SalaryExpectations = resume.SalaryExpectations,
                ExperienceDescription = resume.ExperienceDescription,
                EducationDescription = resume.EducationDescription, // Прямая строка
                Skills = resume.Skills, // Прямая строка
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
            _logger.LogInformation($"=== OnPostAsync для {email} ===");

            var resume = await _context.Resumes.FindAsync(email);
            if (resume == null)
            {
                _logger.LogError("Резюме не найдено в POST!");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState не валиден:");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        _logger.LogWarning($"Ошибка в {error.Key}: {err.ErrorMessage}");
                    }
                }
                return Page();
            }

            try
            {
                // Обновляем прямо из ResumeData
                resume.DesiredPosition = ResumeData.DesiredPosition.Trim();
                resume.SalaryExpectations = ResumeData.SalaryExpectations;
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
                _logger.LogError(ex, "Ошибка при сохранении!");
                ErrorMessage = $"Ошибка при сохранении: {ex.Message}";
                return Page();
            }
        }
    }
}