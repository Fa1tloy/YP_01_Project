using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Helpers;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    /// <summary>
    /// PageModel для личного кабинета работодателя
    /// </summary>
    [Authorize(Roles = "employer")]
    public class EmployerDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IVacancyService _vacancyService;
        private readonly ILogger<EmployerDashboardModel> _logger;

        // Основная информация
        public string UserFirstName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Статистика
        public int TotalVacancies { get; set; }
        public int ActiveVacancies { get; set; }
        public int TotalResponses { get; set; }
        public int NewResponses { get; set; }
        public int ViewsThisWeek { get; set; }

        // Данные для отображения
        public List<VacancyWithStats> Vacancies { get; set; } = new();
        public List<ApplicationDto> RecentApplications { get; set; } = new();
        public List<DailyAnalytic> WeekData { get; set; } = new();

        // DTO для вакансии со статистикой
        public class VacancyWithStats
        {
            public Models.Vacancy Vacancy { get; set; }
            public int ResponseCount { get; set; }
        }

        // DTO для откликов
        public class ApplicationDto
        {
            public string VacancyTitle { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string StudentEmail { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string AppliedAt { get; set; } = string.Empty;
            public string VacancyUrl { get; set; } = string.Empty;
        }

        public EmployerDashboardModel(
            ApplicationDbContext context,
            IVacancyService vacancyService,
            ILogger<EmployerDashboardModel> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _vacancyService = vacancyService ?? throw new ArgumentNullException(nameof(vacancyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult OnGet()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("Доступ без email");
                    return RedirectToPage("/Account/Login");
                }

                _logger.LogInformation("Загрузка дашборда работодателя для {Email}", userEmail);

                UserEmail = userEmail;
                UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Работодатель";

                LoadStatistics(userEmail);
                LoadVacanciesWithStats(userEmail);
                LoadRecentApplications(userEmail);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки дашборда работодателя");
                return RedirectToPage("/Error");
            }
        }

        private void LoadStatistics(string userEmail)
        {
            try
            {
                // Всего вакансий
                TotalVacancies = _context.Vacancies.Count(v => v.AuthorEmail == userEmail);

                // Активные вакансии
                ActiveVacancies = TotalVacancies;

                // Получаем компании работодателя
                var companyNames = _context.Vacancies
                    .Where(v => v.AuthorEmail == userEmail)
                    .Select(v => v.CompanyName)
                    .Distinct()
                    .ToList();

                // Всего откликов по вакансиям этих компаний
                TotalResponses = _context.JobApplications
                    .Count(a => companyNames.Contains(a.VacancyCompanyName));

                // Новые отклики (за последнюю неделю)
                var weekAgo = DateTime.Now.AddDays(-7);
                NewResponses = _context.JobApplications
                    .Count(a => companyNames.Contains(a.VacancyCompanyName) && a.AppliedAt >= weekAgo);

                _logger.LogInformation(
                    "Статистика загружена: Вакансий={TotalVacancies}, Откликов={TotalResponses}, Новых={NewResponses}",
                    TotalVacancies, TotalResponses, NewResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки статистики для {Email}", userEmail);
            }
        }

        private void LoadVacanciesWithStats(string userEmail)
        {
            try
            {
                var baseVacancies = _vacancyService.GetUserVacancies(userEmail).ToList();

                Vacancies = new List<VacancyWithStats>();
                foreach (var vacancy in baseVacancies)
                {
                    var responseCount = _context.JobApplications
                        .Count(a => a.VacancyCompanyName == vacancy.CompanyName && a.VacancyTitle == vacancy.Title);

                    Vacancies.Add(new VacancyWithStats
                    {
                        Vacancy = vacancy,
                        ResponseCount = responseCount
                    });
                }

                _logger.LogInformation("Загружено {Count} вакансий со статистикой для {Email}",
                    Vacancies.Count, userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки вакансий для {Email}", userEmail);
                Vacancies = new List<VacancyWithStats>();
            }
        }

        private void LoadRecentApplications(string userEmail)
        {
            try
            {
                // Получаем компании работодателя
                var companyNames = _context.Vacancies
                    .Where(v => v.AuthorEmail == userEmail)
                    .Select(v => v.CompanyName)
                    .Distinct()
                    .ToList();

                // Получаем отклики по вакансиям этих компаний
                var applications = _context.JobApplications
                    .Where(a => companyNames.Contains(a.VacancyCompanyName))
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(10)
                    .ToList();

                RecentApplications = new List<ApplicationDto>();
                foreach (var app in applications)
                {
                    // Получаем информацию о студенте
                    var student = _context.Users.FirstOrDefault(u => u.Email == app.StudentEmail);

                    // Получаем информацию о вакансии
                    var vacancy = _context.Vacancies.FirstOrDefault(v =>
                        v.CompanyName == app.VacancyCompanyName && v.Title == app.VacancyTitle);

                    if (student != null && vacancy != null)
                    {
                        RecentApplications.Add(new ApplicationDto
                        {
                            VacancyTitle = vacancy.Title,
                            CompanyName = vacancy.CompanyName,
                            StudentName = $"{student.FirstName} {student.LastName}",
                            StudentEmail = student.Email,
                            Status = app.Status,
                            AppliedAt = app.AppliedAt.ToString("dd.MM.yyyy"),
                            VacancyUrl = $"/Vacancy/Details/{Uri.EscapeDataString(vacancy.CompanyName)}/{Uri.EscapeDataString(vacancy.Title)}"
                        });
                    }
                }

                _logger.LogInformation("Загружено {Count} откликов для {Email}",
                    RecentApplications.Count, userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки откликов для {Email}", userEmail);
                RecentApplications = new List<ApplicationDto>();
            }
        }

        /// <summary>
        /// AJAX-метод для получения данных графика
        /// </summary>
        public IActionResult OnGetChartData()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized();

                var data = Enumerable.Range(0, 7).Select(i => new
                {
                    date = DateTime.Now.AddDays(-i).ToString("dd.MM"),
                    responses = new Random().Next(0, 5),
                    views = new Random().Next(0, 10)
                }).Reverse().ToList();

                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения данных графика");
                return new JsonResult(new List<object>());
            }
        }
    }
}