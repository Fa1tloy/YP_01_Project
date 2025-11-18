using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Vacancy
{
    [Authorize(Roles = "employer")]
    public class CreateModel : PageModel
    {
        private readonly IVacancyService _vacancyService;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IVacancyService vacancyService,
                         ICompanyRepository companyRepository,
                         ILogger<CreateModel> logger)
        {
            _vacancyService = vacancyService;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        [BindProperty]
        public CreateVacancyViewModel VacancyData { get; set; } = new();

        public List<WebReckrytingSystem.Models.Company> Companies { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            _logger.LogInformation("GET запрос на страницу создания вакансии");
            LoadCompanies();
            _logger.LogInformation($"Загружено компаний: {Companies.Count}");
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("POST запрос на создание вакансии");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Модель не валидна. Ошибки:");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        _logger.LogWarning($"Ошибка в поле {error.Key}: {err.ErrorMessage}");
                    }
                }

                LoadCompanies();
                return Page();
            }

            var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation($"Email пользователя: {userEmailClaim}");

            if (string.IsNullOrEmpty(userEmailClaim))
            {
                ErrorMessage = "Ошибка аутентификации";
                _logger.LogError("Email пользователя не найден в claims");
                LoadCompanies();
                return Page();
            }

            try
            {
                _logger.LogInformation("Вызов VacancyService.CreateVacancy");
                var result = _vacancyService.CreateVacancy(userEmailClaim, VacancyData);
                _logger.LogInformation($"Результат создания: IsSuccess={result.IsSuccess}, Message={result.Message}");

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Вакансия успешно опубликована!";
                    _logger.LogInformation("Успешное создание вакансии, редирект на EmployerDashboard");
                    return RedirectToPage("/Account/EmployerDashboard");
                }
                else
                {
                    // Добавляем ошибку в ModelState для отображения в форме
                    ModelState.AddModelError("", result.Message);
                    ErrorMessage = result.Message;
                    _logger.LogWarning($"Ошибка создания вакансии: {result.Message}");
                    LoadCompanies();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Исключение при создании вакансии");
                ErrorMessage = "Произошла ошибка при создании вакансии";
                LoadCompanies();
                return Page();
            }
        }
        private void LoadCompanies()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmail))
            {
                Companies = _companyRepository.GetUserCompanies(userEmail).ToList();
                _logger.LogInformation($"Загружено компаний для пользователя {userEmail}: {Companies.Count}");
            }
        }
    }
}