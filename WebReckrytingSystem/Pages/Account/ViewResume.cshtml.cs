using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    public class ViewResumeModel : PageModel
    {
        private readonly IResumeService _resumeService;

        public ViewResumeModel(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        public Resume? Resume { get; set; }
        public string UserFirstName { get; set; } = string.Empty;
        public bool IsOwner { get; set; }

        public string? SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Проверяем сообщение об успехе из TempData
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            // Получаем резюме текущего пользователя
            Resume = _resumeService.GetUserResume(userEmail);

            if (Resume == null)
            {
                return RedirectToPage("/Account/CreateResume");
            }

            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";
            IsOwner = true;

            return Page();
        }
    }
}