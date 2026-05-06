using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(UserService userService, IUserRepository userRepository, ILogger<RegisterModel> logger)
        {
            _userService = userService;
            _userRepository = userRepository;
            _logger = logger;
        }

        [BindProperty]
        public RegisterViewModel RegisterData { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public void OnGet(string? role)
        {
            RegisterData.Role = WebReckrytingSystem.Models.User.ROLE_JOB_SEEKER;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("⚠️ Форма регистрации не валидна");
                return Page();
            }

            var result = _userService.RegisterUser(
                RegisterData.Email,
                RegisterData.Password,
                RegisterData.FirstName,
                RegisterData.LastName,
                WebReckrytingSystem.Models.User.ROLE_JOB_SEEKER
            );

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Пользователь {Email} успешно зарегистрирован", RegisterData.Email);

                // Автоматический вход после регистрации
                await SignInUser(result.Data!);

                // Редирект на дашборд
                return RedirectToDashboard(WebReckrytingSystem.Models.User.ROLE_JOB_SEEKER);
            }
            else
            {
                _logger.LogWarning("⚠️ Ошибка регистрации {Email}: {Message}", RegisterData.Email, result.Message);
                ModelState.AddModelError("", result.Message);
                RegisterData.Password = string.Empty;
                RegisterData.ConfirmPassword = string.Empty;
                return Page();
            }
        }

        // AJAX проверка email
        public IActionResult OnPostCheckEmail([FromBody] string email)
        {
            _logger.LogInformation("📧 Проверка email: {Email}", email);
            var user = _userRepository.FindByEmail(email);
            return new JsonResult(new { exists = user != null });
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

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = false }
            );
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