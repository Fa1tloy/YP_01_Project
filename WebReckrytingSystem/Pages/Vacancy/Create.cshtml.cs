using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
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

        public List<string> CompanySuggestions { get; set; } = new();

        public IReadOnlyList<string> Specialties { get; } = new List<string>
        {
            "Информационные системы и программирование",
            "Сетевое и системное администрирование",
            "Экономика и бухгалтерский учет",
            "Банковское дело",
            "Дизайн",
            "Маркетинг",
            "Юриспруденция",
            "Техническое обслуживание и ремонт автотранспорта",
            "Строительство и эксплуатация зданий и сооружений",
            "Электромонтер",
            "Туризм и гостеприимство"
        };

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

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (!string.IsNullOrWhiteSpace(user?.CompanyName))
            {
                VacancyData.CompanyName = user.CompanyName;
            }

            LoadSuggestions();
            return Page();
        }

        public IActionResult OnPost()
        {
            LoadSuggestions();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            try
            {
                var result = _vacancyService.CreateVacancy(userEmail, VacancyData);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Вакансия успешно создана и опубликована!";
                    return RedirectToPage("/Account/EmployerDashboard");
                }

                ErrorMessage = result.Message;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания вакансии");
                ErrorMessage = "Произошла ошибка при создании вакансии";
                return Page();
            }
        }

        private void LoadSuggestions()
        {
            CompanySuggestions = _context.Companies
                .Select(c => c.Name)
                .OrderBy(n => n)
                .ToList();
        }
    }
}
