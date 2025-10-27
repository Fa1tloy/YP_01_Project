// Pages/Account/JobSeekerDashboard.cshtml.cs
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

        public void OnGet()
        {
            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";

            // Проверяем наличие резюме
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmail))
            {
                var resume = _resumeService.GetUserResume(userEmail);
                HasResume = resume != null;
            }
        }
    }
}