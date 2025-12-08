using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using Microsoft.Extensions.Logging;

namespace WebReckrytingSystem.Pages.Vacancy
{
    [Authorize(Roles = "employer")]
    public class CreateModel : PageModel
    {
        private readonly IVacancyService _vacancyService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public CreateVacancyViewModel VacancyData { get; set; } = new();

        public string UserCompanyName { get; set; } = string.Empty;
        public bool HasCompany { get; set; }

        // ✅ ДОБАВЛЕНЫ отсутствующие свойства
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public CreateModel(
            IVacancyService vacancyService,
            ApplicationDbContext context,
            ILogger<CreateModel> logger)
        {
            _vacancyService = vacancyService;
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var user = _context.Users
                .FirstOrDefault(u => u.Email == userEmail);

            if (user?.CompanyName != null)
            {
                UserCompanyName = user.CompanyName;
                VacancyData.CompanyName = user.CompanyName;
                HasCompany = true;
            }
            else
            {
                TempData["ErrorMessage"] = "Сначала создайте компанию в настройках профиля";
                return RedirectToPage("/Company/Settings");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user?.CompanyName != VacancyData.CompanyName)
            {
                ModelState.AddModelError("", "Вы можете создавать вакансии только от своей компании");
                return Page();
            }

            try
            {
                var result = _vacancyService.CreateVacancy(userEmail, VacancyData);

                if (result.IsSuccess)
                {
                    // ✅ Используем TempData для передачи сообщений
                    TempData["SuccessMessage"] = "Вакансия успешно создана и опубликована!";
                    return RedirectToPage("/Account/EmployerDashboard");
                }
                else
                {
                    ErrorMessage = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания вакансии");
                ErrorMessage = "Произошла ошибка при создании вакансии";
                return Page();
            }
        }
    }
}