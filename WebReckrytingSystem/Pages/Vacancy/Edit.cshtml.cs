using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Vacancy
{
    [Authorize(Roles = "employer")]
    public class EditModel : PageModel
    {
        private readonly IVacancyService _vacancyService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IVacancyService vacancyService,
            ApplicationDbContext context,
            ILogger<EditModel> logger)
        {
            _vacancyService = vacancyService;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public CreateVacancyViewModel VacancyData { get; set; } = new();

        public Models.Vacancy CurrentVacancy { get; set; } = null!;
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

        public IActionResult OnGet(string companyName, string title)
        {
            _logger.LogInformation("GET request to edit vacancy: {Company} - {Title}", companyName, title);

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Unauthorized user tried to access vacancy edit");
                return RedirectToPage("/Account/Login");
            }

            CurrentVacancy = _vacancyService.GetVacancy(companyName, title);
            if (CurrentVacancy == null)
            {
                _logger.LogWarning("Vacancy not found: {Company} - {Title}", companyName, title);
                ErrorMessage = "Вакансия не найдена";
                LoadSuggestions();
                return Page();
            }

            if (CurrentVacancy.AuthorEmail != userEmail)
            {
                _logger.LogWarning(
                    "Attempt to edit vacancy by non-author. Author: {Author}, user: {User}",
                    CurrentVacancy.AuthorEmail,
                    userEmail);
                return RedirectToPage("/AccessDenied");
            }

            VacancyData.CompanyName = CurrentVacancy.CompanyName;
            VacancyData.Region = CurrentVacancy.Region;
            VacancyData.EmploymentType = CurrentVacancy.EmploymentType;
            VacancyData.Title = CurrentVacancy.Title;
            VacancyData.Description = CurrentVacancy.Description;
            VacancyData.Requirements = CurrentVacancy.Requirements;
            VacancyData.WorkSchedule = CurrentVacancy.WorkSchedule;
            VacancyData.WorkHoursPerDay = CurrentVacancy.WorkHoursPerDay;
            VacancyData.WorkFormat = CurrentVacancy.WorkFormat;
            VacancyData.SalaryFrom = CurrentVacancy.SalaryFrom;
            VacancyData.SalaryTo = CurrentVacancy.SalaryTo;
            VacancyData.SalaryPeriod = CurrentVacancy.SalaryPeriod;
            VacancyData.PaymentFrequency = CurrentVacancy.PaymentFrequency;
            VacancyData.Specialty = CurrentVacancy.Specialty;

            LoadSuggestions();
            return Page();
        }

        public IActionResult OnPost(string companyName, string title)
        {
            _logger.LogInformation("POST request to update vacancy: {Company} - {Title}", companyName, title);
            LoadSuggestions();

            if (!ModelState.IsValid)
            {
                CurrentVacancy = _vacancyService.GetVacancy(companyName, title) ?? new Models.Vacancy();
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError(string.Empty, "Ошибка авторизации");
                _logger.LogWarning("No email claim while updating vacancy");
                CurrentVacancy = _vacancyService.GetVacancy(companyName, title) ?? new Models.Vacancy();
                return Page();
            }

            try
            {
                var result = _vacancyService.UpdateVacancy(companyName, title, userEmail, VacancyData);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Вакансия успешно обновлена!";
                    return RedirectToPage("/Account/EmployerDashboard");
                }

                ModelState.AddModelError(string.Empty, result.Message ?? "Не удалось обновить вакансию");
                CurrentVacancy = _vacancyService.GetVacancy(companyName, title) ?? new Models.Vacancy();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating vacancy");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении вакансии");
                CurrentVacancy = _vacancyService.GetVacancy(companyName, title) ?? new Models.Vacancy();
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
