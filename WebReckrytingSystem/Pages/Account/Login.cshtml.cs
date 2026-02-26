using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using Microsoft.Extensions.Logging;

namespace WebReckrytingSystem.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(UserService userService, ILogger<LoginModel> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty]
        public LoginViewModel LoginData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("✅ Пользователь уже аутентифицирован, редирект на дашборд");
                RedirectToDashboard(User.FindFirst(ClaimTypes.Role)?.Value);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("⚠️ Модель не валидна: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return Page();
            }

            try
            {
                _logger.LogInformation("🔐 Попытка входа: {Email}", LoginData.Email);

                var result = _userService.AuthenticateUser(LoginData.Email, LoginData.Password);

                if (result.IsSuccess && result.Data != null)
                {
                    var user = result.Data;

                    // Проверка блокировки
                    if (user.Role == "blocked")
                    {
                        _logger.LogWarning("🚫 Попытка входа заблокированного пользователя: {Email}", user.Email);
                        ErrorMessage = "Ваш аккаунт заблокирован. Обратитесь в поддержку.";
                        return Page();
                    }

                    // Создаем claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.GivenName, user.FirstName),
                        new Claim(ClaimTypes.Surname, user.LastName),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("FullName", $"{user.FirstName} {user.LastName}")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = LoginData.RememberMe,
                        ExpiresUtc = LoginData.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8),
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("✅ Успешный вход: {Email} ({Role})", user.Email, user.Role);

                    // Очистка кэша, чтобы пользователь видел актуальные данные
                    Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                    Response.Headers.Add("Pragma", "no-cache");
                    Response.Headers.Add("Expires", "0");

                    return RedirectToDashboard(user.Role);
                }
                else
                {
                    _logger.LogWarning("❌ Неверный email или пароль: {Email}", LoginData.Email);
                    ErrorMessage = "Неверный email или пароль";
                    LoginData.Password = string.Empty; // Очищаем пароль
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка аутентификации: {Email}", LoginData.Email);
                ErrorMessage = "Произошла ошибка при входе. Попробуйте позже.";
                LoginData.Password = string.Empty;
                return Page();
            }
        }

        private IActionResult RedirectToDashboard(string? role)
        {
            var targetUrl = role switch
            {
                "job_seeker" => Url.Page("/Account/JobSeekerDashboard"),
                "employer" => Url.Page("/Account/EmployerDashboard"),
                "admin" => Url.Page("/Admin/Index"),
                _ => Url.Page("/Index")
            };

            // Если был ReturnUrl - возвращаем туда
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                _logger.LogInformation("🔄 Редирект на исходную страницу: {ReturnUrl}", ReturnUrl);
                return Redirect(ReturnUrl);
            }

            _logger.LogInformation("🔄 Редирект на дашборд: {Role}", role);
            return Redirect(targetUrl);
        }
    }
}