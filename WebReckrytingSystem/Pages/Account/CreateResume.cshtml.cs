using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using Microsoft.Extensions.Logging;

namespace WebReckrytingSystem.Pages.Account
{
    public class CreateResumeModel : PageModel
    {
        private readonly IResumeService _resumeService;
        private readonly ILogger<CreateResumeModel> _logger;

        public CreateResumeModel(IResumeService resumeService, ILogger<CreateResumeModel> logger)
        {
            _resumeService = resumeService;
            _logger = logger;
        }

        [BindProperty]
        public CreateResumeViewModel ResumeData { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.FindFirst(ClaimTypes.Role)?.Value != "job_seeker")
            {
                return RedirectToPage("/AccessDenied");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("=== НАЧАЛО ОБРАБОТКИ POST ===");

            // Читаем скрытое поле навыков
            var skillsHidden = Request.Form["SkillsHidden"].FirstOrDefault();
            _logger.LogInformation($"Получен SkillsHidden: {skillsHidden}");

            if (!string.IsNullOrEmpty(skillsHidden))
            {
                try
                {
                    ResumeData.Skills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(skillsHidden) ?? new List<string>();
                    _logger.LogInformation($"Десериализовано навыков: {ResumeData.Skills.Count}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка десериализации навыков");
                    ResumeData.Skills = new List<string>();
                }
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState не валиден:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Ошибка: {error.ErrorMessage}");
                }
                ModelState.Remove("ResumeData.Skills");
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                ErrorMessage = "Ошибка аутентификации";
                return Page();
            }

            var result = _resumeService.CreateResume(userEmail, ResumeData);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Резюме успешно создано и опубликовано!";
                return RedirectToPage("/Account/ViewResume");
            }
            else
            {
                ErrorMessage = result.Message;
                ModelState.Remove("ResumeData.Skills");
                return Page();
            }
        }

        // Методы для AJAX остаются без изменений
        public IActionResult OnPostAddSkill([FromBody] string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
                return new JsonResult(new { success = false, message = "Навык не может быть пустым" });

            if (ResumeData.Skills.Count >= 20)
                return new JsonResult(new { success = false, message = "Максимум 20 навыков" });

            if (ResumeData.Skills.Contains(skill, StringComparer.OrdinalIgnoreCase))
                return new JsonResult(new { success = false, message = "Такой навык уже добавлен" });

            ResumeData.Skills.Add(skill);
            return new JsonResult(new { success = true, skills = ResumeData.Skills });
        }

        public IActionResult OnPostRemoveSkill([FromBody] string skill)
        {
            ResumeData.Skills.RemoveAll(s => s.Equals(skill, StringComparison.OrdinalIgnoreCase));
            return new JsonResult(new { success = true, skills = ResumeData.Skills });
        }
    }
}