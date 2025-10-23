using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebReckrytingSystem.Pages.Account
{
    public class JobSeekerDashboardModel : PageModel
    {
        public string UserFirstName { get; set; } = string.Empty;

        public void OnGet()
        {
            // Здесь будет логика получения данных пользователя
            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";
        }
    }
}