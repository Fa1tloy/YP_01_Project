// Pages/Account/Login.cshtml.cs
public class LoginModel : PageModel
{
    private readonly UserService _userService;

    [BindProperty]
    public LoginViewModel LoginData { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // 1. Аутентификация пользователя
        var result = _userService.AuthenticateUser(LoginData.Email, LoginData.Password);

        if (result.IsSuccess && result.Data != null)
        {
            // 2. Создание claims (данных пользователя)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, result.Data.Email),
                new Claim(ClaimTypes.GivenName, result.Data.FirstName),
                new Claim(ClaimTypes.Surname, result.Data.LastName),
                new Claim(ClaimTypes.Role, result.Data.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // 3. Настройка свойств куки
            var authProperties = new AuthenticationProperties();

            if (LoginData.RememberMe)
            {
                // Кука на 30 дней
                authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
                authProperties.IsPersistent = true;
            }
            else
            {
                // Сессионная кука
                authProperties.IsPersistent = false;
            }

            // 4. Создание аутентификационной куки
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 5. Редирект в соответствующий кабинет
            return result.Data.Role switch
            {
                "job_seeker" => RedirectToPage("/Account/JobSeekerDashboard"),
                "employer" => RedirectToPage("/Account/EmployerDashboard"),
                _ => RedirectToPage("/Index")
            };
        }
        else
        {
            ErrorMessage = result.Message;
            LoginData.Password = string.Empty; // Очищаем пароль при ошибке
            return Page();
        }
    }
}