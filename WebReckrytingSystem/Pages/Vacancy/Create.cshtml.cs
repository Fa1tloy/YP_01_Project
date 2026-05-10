using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using Microsoft.Extensions.Logging;

namespace WebReckrytingSystem.Pages.Vacancy
{
    [Authorize(Roles = "admin")]
    public class CreateModel : PageModel
    {
        private readonly IVacancyService _vacancyService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public CreateVacancyViewModel VacancyData { get; set; } = new();

        public List<SelectListItem> CompanyOptions { get; set; } = new();

        public IReadOnlyList<string> Specialties => SpecialtyCatalog.All;

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
                    return RedirectToPage("/Admin/Vacancies/Vacancies");
                }

                var errorMessage = string.IsNullOrWhiteSpace(result.Message)
                    ? "Не удалось создать вакансию"
                    : result.Message;
                ModelState.AddModelError(string.Empty, errorMessage);
                ErrorMessage = errorMessage;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания вакансии");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании вакансии");
                ErrorMessage = "Произошла ошибка при создании вакансии";
                return Page();
            }
        }

        private void LoadSuggestions()
        {
            CompanyOptions = _context.Companies
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name
                })
                .ToList();
        }
    }
}
