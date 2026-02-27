using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WebReckrytingSystem.Helpers;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Account
{
    /// <summary>
    /// PageModel для личного кабинета соискателя
    /// </summary>
    [Authorize(Roles = "job_seeker")]
    public class JobSeekerDashboardModel : PageModel
    {
        private readonly WebReckrytingSystem.Models.ApplicationDbContext _context;
        private readonly IResumeService _resumeService;
        private readonly IVacancySearchService _vacancySearchService;
        private readonly ILogger<JobSeekerDashboardModel> _logger;

        // Основная информация
        public string UserFirstName { get; set; } = string.Empty;
        public Models.Resume? UserResume { get; set; }
        public bool HasResume { get; set; }

        // Статистика
        public int TotalApplications { get; set; }
        public int TotalViews { get; set; }
        public int SavedVacanciesCount { get; set; }
        public int WeekViews { get; set; }
        public int MonthViews { get; set; }

        // Данные для отображения
        public List<DailyAnalytic> WeekData { get; set; } = new();
        public List<SavedVacancyDto> SavedVacanciesDto { get; set; } = new();
        public List<ApplicationDto> ApplicationsDto { get; set; } = new();

        // DTO для передачи данных в представление
        public class SavedVacancyDto
        {
            public string Title { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string SavedAt { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
        }

        public class ApplicationDto
        {
            public string Title { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string AppliedAt { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
        }

        public JobSeekerDashboardModel(
            WebReckrytingSystem.Models.ApplicationDbContext context,
            IResumeService resumeService,
            IVacancySearchService vacancySearchService,
            ILogger<JobSeekerDashboardModel> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _resumeService = resumeService ?? throw new ArgumentNullException(nameof(resumeService));
            _vacancySearchService = vacancySearchService ?? throw new ArgumentNullException(nameof(vacancySearchService));
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

                _logger.LogInformation("Загрузка дашборда для {Email}", userEmail);

                UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Студент";
                UserResume = _resumeService.GetUserResume(userEmail);
                HasResume = UserResume != null;

                LoadStatistics(userEmail);
                LoadSavedVacancies(userEmail);
                LoadRecentApplications(userEmail);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки дашборда");
                return RedirectToPage("/Error");
            }
        }

        private void LoadStatistics(string userEmail)
        {
            try
            {
                TotalApplications = _context.JobApplications.Count(a => a.StudentEmail == userEmail);
                TotalViews = _context.ResumeViews.Count(v => v.ResumeEmail == userEmail);
                SavedVacanciesCount = _context.SavedVacancies.Count(s => s.StudentEmail == userEmail);

                var weekAgo = DateTime.Now.AddDays(-7);
                WeekViews = _context.ResumeViews.Count(v => v.ResumeEmail == userEmail && v.ViewedAt >= weekAgo);

                var monthAgo = DateTime.Now.AddDays(-30);
                MonthViews = _context.ResumeViews.Count(v => v.ResumeEmail == userEmail && v.ViewedAt >= monthAgo);

                WeekData = _context.DailyAnalytics
                    .Where(d => d.UserEmail == userEmail && d.Date >= weekAgo)
                    .OrderBy(d => d.Date)
                    .ToList();

                if (!WeekData.Any())
                {
                    WeekData = Enumerable.Range(0, 7).Select(i => new DailyAnalytic
                    {
                        Date = DateTime.Now.AddDays(-i).Date,
                        ProfileViews = 0,
                        ApplicationsSent = 0,
                        SavedVacancies = 0
                    }).Reverse().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки статистики");
            }
        }

        private void LoadSavedVacancies(string userEmail)
        {
            try
            {
                var savedIds = _context.SavedVacancies
                    .Where(s => s.StudentEmail == userEmail)
                    .OrderByDescending(s => s.SavedAt)
                    .Take(10)
                    .Select(s => new { s.VacancyCompanyName, s.VacancyTitle, s.SavedAt })
                    .ToList();

                SavedVacanciesDto = new List<SavedVacancyDto>();
                foreach (var id in savedIds)
                {
                    var vacancy = _context.Vacancies
                        .Include(v => v.Company)
                        .FirstOrDefault(v => v.CompanyName == id.VacancyCompanyName && v.Title == id.VacancyTitle);

                    if (vacancy != null)
                    {
                        SavedVacanciesDto.Add(new SavedVacancyDto
                        {
                            Title = vacancy.Title,
                            CompanyName = vacancy.CompanyName,
                            SavedAt = id.SavedAt.ToString("dd.MM.yyyy"),
                            Url = $"/Vacancy/Details/{Uri.EscapeDataString(vacancy.CompanyName)}/{Uri.EscapeDataString(vacancy.Title)}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки сохраненных вакансий");
            }
        }

        private void LoadRecentApplications(string userEmail)
        {
            try
            {
                var applications = _context.JobApplications
                    .Where(a => a.StudentEmail == userEmail)
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(10)
                    .ToList();

                ApplicationsDto = new List<ApplicationDto>();
                foreach (var app in applications)
                {
                    var vacancy = _context.Vacancies
                        .Include(v => v.Company)
                        .FirstOrDefault(v => v.CompanyName == app.VacancyCompanyName && v.Title == app.VacancyTitle);

                    if (vacancy != null)
                    {
                        ApplicationsDto.Add(new ApplicationDto
                        {
                            Title = vacancy.Title,
                            CompanyName = vacancy.CompanyName,
                            Status = app.Status,
                            AppliedAt = app.AppliedAt.ToString("dd.MM.yyyy"),
                            Url = $"/Vacancy/Details/{Uri.EscapeDataString(vacancy.CompanyName)}/{Uri.EscapeDataString(vacancy.Title)}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки откликов");
            }
        }

        public IActionResult OnGetChartData()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized();

                var weekAgo = DateTime.Now.AddDays(-7);
                var data = _context.DailyAnalytics
                    .Where(d => d.UserEmail == userEmail && d.Date >= weekAgo)
                    .Select(d => new
                    {
                        date = d.Date.ToString("dd.MM"),
                        views = d.ProfileViews,
                        applications = d.ApplicationsSent,
                        saved = d.SavedVacancies
                    })
                    .ToList();

                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения данных графика");
                return new JsonResult(new List<object>());
            }
        }

        public IActionResult OnGetVacancyRecommendations()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized();

                var userResume = _resumeService.GetUserResume(userEmail);
                if (userResume == null)
                {
                    var fallback = _context.Vacancies
                        .Include(v => v.Company)
                        .Where(v => v.Company.Verified)
                        .OrderByDescending(v => v.CompanyName)
                        .Take(6)
                        .Select(v => new
                        {
                            title = v.Title,
                            companyName = v.CompanyName,
                            salary = v.SalaryFrom.HasValue ? $"{v.SalaryFrom.Value:N0} ₽" : "Договорная",
                            employmentType = ViewHelper.GetEmploymentTypeDisplay(v.EmploymentType),
                            workSchedule = ViewHelper.GetWorkScheduleDisplay(v.WorkSchedule),
                            description = ViewHelper.Truncate(v.Description, 100),
                            url = $"/Vacancy/Details/{Uri.EscapeDataString(v.CompanyName)}/{Uri.EscapeDataString(v.Title)}"
                        }).ToList();

                    return new JsonResult(fallback);
                }

                var keywords = $"{userResume.DesiredPosition} {userResume.Skills}";
                var searchModel = new SearchVacancyViewModel
                {
                    Keywords = keywords.Length > 100 ? keywords.Substring(0, 100) : keywords,
                    Page = 1,
                    PageSize = 6
                };

                var result = _vacancySearchService.SearchVacancies(searchModel);

                if (result.IsSuccess && result.Data?.Items.Any() == true)
                {
                    var recommendations = result.Data.Items.Select(v => new
                    {
                        title = v.Title,
                        companyName = v.CompanyName,
                        salary = v.SalaryFrom.HasValue ? $"{v.SalaryFrom.Value:N0} ₽" : "Договорная",
                        employmentType = ViewHelper.GetEmploymentTypeDisplay(v.EmploymentType),
                        workSchedule = ViewHelper.GetWorkScheduleDisplay(v.WorkSchedule),
                        description = ViewHelper.Truncate(v.Description, 100),
                        url = $"/Vacancy/Details/{Uri.EscapeDataString(v.CompanyName)}/{Uri.EscapeDataString(v.Title)}"
                    }).ToList();

                    return new JsonResult(recommendations);
                }

                return new JsonResult(new List<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения рекомендаций");
                return new JsonResult(new List<object>());
            }
        }

        public IActionResult OnGetSavedVacanciesList()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized();

                var savedIds = _context.SavedVacancies
                    .Where(s => s.StudentEmail == userEmail)
                    .OrderByDescending(s => s.SavedAt)
                    .Take(10)
                    .Select(s => new { s.VacancyCompanyName, s.VacancyTitle, s.SavedAt })
                    .ToList();

                var result = new List<object>();
                foreach (var id in savedIds)
                {
                    var vacancy = _context.Vacancies
                        .Include(v => v.Company)
                        .FirstOrDefault(v => v.CompanyName == id.VacancyCompanyName && v.Title == id.VacancyTitle);

                    if (vacancy != null)
                    {
                        result.Add(new
                        {
                            title = vacancy.Title,
                            companyName = vacancy.CompanyName,
                            savedAt = id.SavedAt.ToString("dd.MM.yyyy"),
                            url = $"/Vacancy/Details/{Uri.EscapeDataString(vacancy.CompanyName)}/{Uri.EscapeDataString(vacancy.Title)}"
                        });
                    }
                }

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения сохраненных вакансий");
                return new JsonResult(new List<object>());
            }
        }

        public IActionResult OnGetJobApplicationsList()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized();

                var applications = _context.JobApplications
                    .Where(a => a.StudentEmail == userEmail)
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(10)
                    .ToList();

                var result = new List<object>();
                foreach (var app in applications)
                {
                    var vacancy = _context.Vacancies
                        .Include(v => v.Company)
                        .FirstOrDefault(v => v.CompanyName == app.VacancyCompanyName && v.Title == app.VacancyTitle);

                    if (vacancy != null)
                    {
                        result.Add(new
                        {
                            title = vacancy.Title,
                            companyName = vacancy.CompanyName,
                            status = app.Status,
                            appliedAt = app.AppliedAt.ToString("dd.MM.yyyy"),
                            url = $"/Vacancy/Details/{Uri.EscapeDataString(vacancy.CompanyName)}/{Uri.EscapeDataString(vacancy.Title)}"
                        });
                    }
                }

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения откликов");
                return new JsonResult(new List<object>());
            }
        }
    }
}
