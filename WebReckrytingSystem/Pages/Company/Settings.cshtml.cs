using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // ✅ ДОБАВЛЕНО для Include
using System.Security.Claims;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

// ✅ ИСПРАВЛЕНО: Добавляем using для разрешения конфликта имен
using CompanyEntity = WebReckrytingSystem.Models.Company;

namespace WebReckrytingSystem.Pages.Company
{
    [Authorize(Roles = "employer")]
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompanyService _companyService;
        private readonly ILogger<SettingsModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public CompanySettingsViewModel CompanyData { get; set; } = new();
        [BindProperty]
        public IFormFile? LogoFile { get; set; }

        public CompanyEntity? CurrentCompany { get; set; } // ✅ ИСПРАВЛЕНО: Явно указываем тип
        public bool HasCompany { get; set; }

        public SettingsModel(
            ApplicationDbContext context,
            ICompanyService companyService,
            ILogger<SettingsModel> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _companyService = companyService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public void OnGet()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                RedirectToPage("/Account/Login");
                return;
            }

            // ✅ ИСПРАВЛЕНО: Явно указываем тип
            var user = _context.Users
                .Include(u => u.Company)
                .FirstOrDefault(u => u.Email == userEmail);

            // ✅ ИСПРАВЛЕНО: Правильная проверка на null
            HasCompany = user?.Company != null;
            CurrentCompany = user?.Company;

            if (CurrentCompany != null)
            {
                // ✅ ИСПРАВЛЕНО: Безопасный доступ к свойствам
                CompanyData = new CompanySettingsViewModel
                {
                    Name = CurrentCompany.Name,
                    Description = CurrentCompany.Description ?? "",
                    Website = CurrentCompany.Website ?? "",
                    LogoUrl = CurrentCompany.LogoUrl ?? ""
                };
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // ✅ ИСПРАВЛЕНО: Проверяем существование компании
                if (string.IsNullOrEmpty(user.CompanyName))
                {
                    // Создаем новую компанию
                    var result = _companyService.CreateCompany(userEmail, new CreateCompanyViewModel
                    {
                        Name = CompanyData.Name,
                        Description = CompanyData.Description,
                        Website = CompanyData.Website
                    });

                    if (!result.IsSuccess)
                    {
                        ModelState.AddModelError("", result.Message);
                        return Page();
                    }

                    // Обновляем пользователя
                    user.CompanyName = CompanyData.Name;
                    if (LogoFile != null && LogoFile.Length > 0)
                    {
                        var createdCompany = _context.Companies.Find(CompanyData.Name);
                        if (createdCompany != null)
                        {
                            createdCompany.LogoUrl = SaveLogoFile(LogoFile);
                        }
                    }
                    _context.SaveChanges();
                }
                else
                {
                    // ✅ ИСПРАВЛЕНО: Обновляем существующую компанию
                    var company = _context.Companies.Find(user.CompanyName);
                    if (company != null)
                    {
                        company.Description = CompanyData.Description;
                        company.Website = CompanyData.Website;
                        if (LogoFile != null && LogoFile.Length > 0)
                        {
                            company.LogoUrl = SaveLogoFile(LogoFile);
                        }
                        _context.SaveChanges();
                    }
                }

                TempData["SuccessMessage"] = "Настройки компании сохранены!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения настроек компании");
                ModelState.AddModelError("", "Произошла ошибка при сохранении");
                return Page();
            }
        }

        private string SaveLogoFile(IFormFile file)
        {
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "companies");
            Directory.CreateDirectory(uploadsDir);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            using var stream = System.IO.File.Create(fullPath);
            file.CopyTo(stream);

            return $"/uploads/companies/{fileName}";
        }
    }

    // ✅ ИСПРАВЛЕНО: ViewModel удалена из класса в отдельный файл
}
