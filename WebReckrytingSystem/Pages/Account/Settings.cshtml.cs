using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly UserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SettingsModel(
            UserService userService,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? AvatarFile { get; set; }

        public string CurrentAvatarUrl { get; set; } = "/images/student.png";
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            LoadCurrentAvatar();
            return Page();
        }

        public IActionResult OnPostPassword()
        {
            if (!ModelState.IsValid)
            {
                LoadCurrentAvatar();
                return Page();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var result = _userService.ChangePassword(userEmail, Input.OldPassword, Input.NewPassword);
            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                ModelState.Clear();
                Input = new ChangePasswordInputModel();
                LoadCurrentAvatar();
                return Page();
            }

            ErrorMessage = result.Message;
            LoadCurrentAvatar();
            return Page();
        }

        public IActionResult OnPostAvatar()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return RedirectToPage("/Account/Login");

            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                ErrorMessage = "Выберите изображение для загрузки.";
                LoadCurrentAvatar();
                return Page();
            }

            if (!AvatarFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Допускаются только файлы изображений.";
                LoadCurrentAvatar();
                return Page();
            }

            try
            {
                var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsDir);

                var extension = Path.GetExtension(AvatarFile.FileName);
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var fullPath = Path.Combine(uploadsDir, fileName);

                using var stream = System.IO.File.Create(fullPath);
                AvatarFile.CopyTo(stream);

                user.AvatarUrl = $"/uploads/avatars/{fileName}";
                _context.SaveChanges();

                CurrentAvatarUrl = user.AvatarUrl;
                SuccessMessage = "Аватар успешно обновлён.";
                return Page();
            }
            catch
            {
                ErrorMessage = "Не удалось загрузить аватар. Попробуйте снова.";
                LoadCurrentAvatar();
                return Page();
            }
        }

        private void LoadCurrentAvatar()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                CurrentAvatarUrl = "/images/student.png";
                return;
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                CurrentAvatarUrl = "/images/student.png";
                return;
            }

            CurrentAvatarUrl = !string.IsNullOrWhiteSpace(user.AvatarUrl)
                ? user.AvatarUrl
                : GetDefaultAvatarByRole(user.Role);
        }

        private static string GetDefaultAvatarByRole(string role) =>
            role == Models.User.ROLE_EMPLOYER ? "/images/rabotodatel.jpg" : "/images/student.png";

        public class ChangePasswordInputModel
        {
            [Required(ErrorMessage = "Введите старый пароль")]
            [DataType(DataType.Password)]
            [Display(Name = "Старый пароль")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите новый пароль")]
            [MinLength(8, ErrorMessage = "Новый пароль должен содержать минимум 8 символов")]
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
