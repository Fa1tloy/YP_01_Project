using Microsoft.AspNetCore.Authorization;  
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize(Roles = "employer")]    // Только для работодателей
    public class EmployerDashboardModel : PageModel
    {
        public string UserFirstName { get; set; } = string.Empty;

        public void OnGet()
        {
            // Дополнительная проверка
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToPage("/Account/Login");
                return;
            }

            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";
        }
    }
}