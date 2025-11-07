using Microsoft.AspNetCore.Authorization;  // ? ДОБАВИТЬ ЭТУ СТРОЧКУ
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    
    public class JobSeekerDashboardModel : PageModel
    {
        private readonly IResumeService _resumeService;

        public JobSeekerDashboardModel(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        public string UserFirstName { get; set; } = string.Empty;
        public bool HasResume { get; set; }

        public IActionResult OnGet()
        {
            // Дополнительная проверка (на всякий случай)
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmail))
            {
                var resume = _resumeService.GetUserResume(userEmail);
                HasResume = resume != null;
            }

            return Page();
        }
    }
}