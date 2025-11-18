using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize(Roles = "employer")]
    public class EmployerDashboardModel : PageModel
    {
        private readonly IVacancyService _vacancyService;
        private readonly ILogger<EmployerDashboardModel> _logger;

        public EmployerDashboardModel(IVacancyService vacancyService, ILogger<EmployerDashboardModel> logger)
        {
            _vacancyService = vacancyService;
            _logger = logger;
        }

        public string UserFirstName { get; set; } = string.Empty;
        public List<Models.Vacancy> Vacancies { get; set; } = new();
        public int ActiveVacanciesCount => Vacancies.Count;

        public IActionResult OnGet()
        {
            // Проверка аутентификации
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Пользователь";

            // Загрузка вакансий пользователя
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmail))
            {
                _logger.LogInformation($"Загрузка вакансий для пользователя: {userEmail}");
                Vacancies = _vacancyService.GetUserVacancies(userEmail).ToList();
                _logger.LogInformation($"Загружено вакансий: {Vacancies.Count}");
            }

            // Проверяем сообщение об успешном создании
            if (TempData["SuccessMessage"] != null)
            {
                ViewData["SuccessMessage"] = TempData["SuccessMessage"];
            }

            return Page();
        }
    }
}