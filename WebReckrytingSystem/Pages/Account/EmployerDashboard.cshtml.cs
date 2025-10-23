using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebReckrytingSystem.Pages.Account
{
    public class EmployerDashboardModel : PageModel
    {
        public string UserFirstName { get; set; } = string.Empty;

        public void OnGet()
        {
            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";
        }
    }
}