using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize(Roles = "employer")]
    public class EmployerDashboardModel : PageModel
    {
        public string UserFirstName { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            // Проверка аутентификации
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";
            return Page();
        }
    }
}