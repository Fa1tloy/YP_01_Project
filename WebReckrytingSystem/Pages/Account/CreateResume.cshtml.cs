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
        private readonly ISpecialtyService _specialtyService;
        private readonly ILogger<CreateResumeModel> _logger;

        public CreateResumeModel(IResumeService resumeService,
                                 ISpecialtyService specialtyService,
                                 ILogger<CreateResumeModel> logger)
        {
            _resumeService = resumeService;
            _specialtyService = specialtyService;
            _logger = logger;
        }

        [BindProperty]
        public CreateResumeViewModel ResumeData { get; set; } = new();
        public IReadOnlyList<string> Specialties { get; set; } = new List<string>();
        public IReadOnlyList<string> DriverLicenseCategories => DriverLicenseCategoryCatalog.All;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.FindFirst(ClaimTypes.Role)?.Value != "job_seeker")
                return RedirectToPage("/AccessDenied");

            Specialties = _specialtyService.GetAllNames();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Specialties = _specialtyService.GetAllNames();

            _logger.LogInformation("=== НАЧАЛО ОБРАБОТКИ POST ===");

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
                ModelState.AddModelError(string.Empty, "Ошибка аутентификации");
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
                var errorMessage = string.IsNullOrWhiteSpace(result.Message)
                    ? "Не удалось создать резюме"
                    : result.Message;
                ModelState.AddModelError(string.Empty, errorMessage);
                ErrorMessage = errorMessage;
                ModelState.Remove("ResumeData.Skills");
                return Page();
            }
        }

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