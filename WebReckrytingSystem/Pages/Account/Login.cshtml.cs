using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace WebReckrytingSystem.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;

        public LoginModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public LoginViewModel LoginData { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = _userService.AuthenticateUser(
                    LoginData.Email,
                    LoginData.Password
                );

                if (result.IsSuccess && result.Data != null)
                {
                    // ??????? claims ??? ??????????????
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, result.Data.Email),
                new Claim(ClaimTypes.GivenName, result.Data.FirstName),
                new Claim(ClaimTypes.Surname, result.Data.LastName),
                new Claim(ClaimTypes.Role, result.Data.Role)
            };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = LoginData.RememberMe
                    };

                    if (LoginData.RememberMe)
                    {
                        // ???? ?? 30 ???? ??? "????????? ????"
                        authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
                        authProperties.IsPersistent = true;
                    }
                    else
                    {
                        // ??????? ?????????? ???? (???????? ??? ???????? ????????)
                        authProperties.IsPersistent = false;
                    }

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToDashboard(result.Data.Role);
                }
                else
                {
                    // ????????????? ????????? ?? ??????
                    ErrorMessage = result.Message;

                    // ????????? email ??? ???????? ????????????
                    // ??????? ?????? ??????
                    LoginData.Password = string.Empty;

                    return Page();
                }
            }
            catch (Exception ex)
            {
                // ????????? ?????????????? ??????
                ErrorMessage = "????????? ?????? ??? ?????. ??????????, ?????????? ?????.";

                // ???????? ??? ????????????
                Console.WriteLine($"?????? ? Login: {ex.Message}");

                LoginData.Password = string.Empty;
                return Page();
            }
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