using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace WebReckrytingSystem.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;

        public RegisterModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public RegisterViewModel RegisterData { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public void OnGet(string? role)
        {
            if (!string.IsNullOrEmpty(role) && (role == "job_seeker" || role == "employer"))
            {
                RegisterData.Role = role;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Показываем ошибки валидации модели
                return Page();
            }

            var result = _userService.RegisterUser(
                RegisterData.Email,
                RegisterData.Password,
                RegisterData.FirstName,
                RegisterData.LastName,
                RegisterData.Role
            );

            if (result.IsSuccess)
            {
                // Автоматический вход после регистрации
                await SignInUser(result.Data!);

                // Редирект в соответствующий кабинет
                return RedirectToDashboard(RegisterData.Role);
            }
            else
            {
                // Добавляем понятное сообщение об ошибке
                ModelState.AddModelError("", result.Message);

                // Сохраняем введенные данные (кроме пароля) для удобства пользователя
                RegisterData.Password = string.Empty;
                RegisterData.ConfirmPassword = string.Empty;

                return Page();
            }
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private IActionResult RedirectToDashboard(string role)
        {
            return role switch
            {
                "job_seeker" => RedirectToPage("/Account/JobSeekerDashboard"),
                "employer" => RedirectToPage("/Account/EmployerDashboard"),
                _ => RedirectToPage("/Index")
            };
        }
    }
}