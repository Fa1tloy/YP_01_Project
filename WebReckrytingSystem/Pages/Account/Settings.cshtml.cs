// Pages/Account/Settings.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly UserService _userService;

        public SettingsModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var result = _userService.ChangePassword(userEmail, Input.OldPassword, Input.NewPassword);
            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                ModelState.Clear();
                Input = new ChangePasswordInputModel();
                return Page();
            }
            else
            {
                ErrorMessage = result.Message;
                return Page();
            }
        }

        public class ChangePasswordInputModel
        {
            [Required(ErrorMessage = "Введите старый пароль")]
            [DataType(DataType.Password)]
            [Display(Name = "Старый пароль")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите новый пароль")]
            [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
            [DataType(DataType.Password)]
            [Display(Name = "Новый пароль")]
            public string NewPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Подтвердите новый пароль")]
            [DataType(DataType.Password)]
            [Display(Name = "Подтверждение пароля")]
            [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}