// Pages/Account/CreateResume.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    public class CreateResumeModel : PageModel
    {
        private readonly IResumeService _resumeService;

        public CreateResumeModel(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [BindProperty]
        public CreateResumeViewModel ResumeData { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Проверяем, что пользователь - соискатель
            if (User.FindFirst(ClaimTypes.Role)?.Value != "job_seeker")
            {
                return RedirectToPage("/AccessDenied");
            }

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

            var result = _resumeService.CreateResume(userEmail, ResumeData);

            if (result.IsSuccess)
            {
                // Успешное создание - редирект на просмотр резюме
                TempData["SuccessMessage"] = "Резюме успешно создано и опубликовано!";
                return RedirectToPage("/Account/ViewResume");
            }
            else
            {
                ErrorMessage = result.Message;
                return Page();
            }
        }

        // Методы для AJAX остаются без изменений
        public IActionResult OnPostAddSkill([FromBody] string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
            {
                return new JsonResult(new { success = false, message = "Навык не может быть пустым" });
            }

            if (ResumeData.Skills.Count >= 20)
            {
                return new JsonResult(new { success = false, message = "Максимум 20 навыков" });
            }

            if (ResumeData.Skills.Contains(skill, StringComparer.OrdinalIgnoreCase))
            {
                return new JsonResult(new { success = false, message = "Такой навык уже добавлен" });
            }

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