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
    public class EditModel : PageModel
    {
        private readonly IVacancyService _vacancyService;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IVacancyService vacancyService,
                        ICompanyRepository companyRepository,
                        ILogger<EditModel> logger)
        {
            _vacancyService = vacancyService;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        [BindProperty]
        public CreateVacancyViewModel VacancyData { get; set; } = new();

        public Models.Vacancy CurrentVacancy { get; set; } = null!;
        public List<Models.Company> Companies { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(string companyName, string title)
        {
            _logger.LogInformation($"GET запрос на редактирование вакансии: {companyName} - {title}");

            // Получение email пользователя
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Пользователь не аутентифицирован");
                return RedirectToPage("/Account/Login");
            }

            // Загрузка вакансии
            CurrentVacancy = _vacancyService.GetVacancy(companyName, title);
            if (CurrentVacancy == null)
            {
                _logger.LogWarning($"Вакансия не найдена: {companyName} - {title}");
                ErrorMessage = "Вакансия не найдена";
                return Page();
            }

            // Проверка авторства
            if (CurrentVacancy.AuthorEmail != userEmail)
            {
                _logger.LogWarning($"Попытка редактирования чужой вакансии. Автор: {CurrentVacancy.AuthorEmail}, Пользователь: {userEmail}");
                return RedirectToPage("/AccessDenied");
            }

            // Предзаполнение формы
            VacancyData.CompanyName = CurrentVacancy.CompanyName;
            VacancyData.Title = CurrentVacancy.Title;
            VacancyData.Description = CurrentVacancy.Description;
            VacancyData.Requirements = CurrentVacancy.Requirements;
            VacancyData.SalaryFrom = CurrentVacancy.SalaryFrom;
            VacancyData.SalaryTo = CurrentVacancy.SalaryTo;
            VacancyData.EmploymentType = CurrentVacancy.EmploymentType;
            VacancyData.WorkSchedule = CurrentVacancy.WorkSchedule;

            // Загрузка компаний пользователя
            LoadCompanies(userEmail);

            _logger.LogInformation($"Форма редактирования загружена для вакансии: {CurrentVacancy.Title}");
            return Page();
        }

        public IActionResult OnPost(string companyName, string title)
        {
            _logger.LogInformation($"POST запрос на обновление вакансии: {companyName} - {title}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Модель не валидна");

                // Перезагрузка данных для отображения формы
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    LoadCompanies(userEmail);
                    CurrentVacancy = _vacancyService.GetVacancy(companyName, title) ?? new Models.Vacancy();
                }

                return Page();
            }

            var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmailClaim))
            {
                ErrorMessage = "Ошибка аутентификации";
                _logger.LogError("Email пользователя не найден в claims");
                return Page();
            }

            try
            {
                _logger.LogInformation("Вызов VacancyService.UpdateVacancy");
                var result = _vacancyService.UpdateVacancy(companyName, title, userEmailClaim, VacancyData);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Вакансия успешно обновлена");
                    TempData["SuccessMessage"] = "Вакансия успешно обновлена!";
                    return RedirectToPage("/Account/EmployerDashboard");
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning($"Ошибка обновления вакансии: {result.Message}");

                    // Перезагрузка данных для отображения формы
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        LoadCompanies(userEmail);
                        CurrentVacancy = _vacancyService.GetVacancy(companyName, title) ?? new Models.Vacancy();
                    }

                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Исключение при обновлении вакансии");
                ErrorMessage = "Произошла ошибка при обновлении вакансии";

                // Перезагрузка данных для отображения формы
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    LoadCompanies(userEmail);
                    CurrentVacancy = _vacancyService.GetVacancy(companyName, title) ?? new Models.Vacancy();
                }

                return Page();
            }
        }

        private void LoadCompanies(string userEmail)
        {
            Companies = _companyRepository.GetUserCompanies(userEmail).ToList();
            _logger.LogInformation($"Загружено компаний для пользователя {userEmail}: {Companies.Count}");
        }
    }
}