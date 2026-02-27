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
    /// PageModel для личного кабинета работодателя
    /// </summary>
    [Authorize(Roles = "employer")]
    public class EmployerDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IVacancyService _vacancyService;
        private readonly ILogger<EmployerDashboardModel> _logger;

        // Îñíîâíàÿ èíôîðìàöèÿ
        public string UserFirstName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Ñòàòèñòèêà
        public int TotalVacancies { get; set; }
        public int ActiveVacancies { get; set; }
        public int TotalResponses { get; set; }
        public int NewResponses { get; set; }
        public int ViewsThisWeek { get; set; }

        // Äàííûå äëÿ îòîáðàæåíèÿ
        public List<VacancyWithStats> Vacancies { get; set; } = new();
        public List<ApplicationDto> RecentApplications { get; set; } = new();
        public List<DailyAnalytic> WeekData { get; set; } = new();

        // DTO äëÿ âàêàíñèè ñî ñòàòèñòèêîé
        public class VacancyWithStats
        {
            public Models.Vacancy Vacancy { get; set; }
            public int ResponseCount { get; set; }
        }

        // DTO äëÿ îòêëèêîâ
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
                    _logger.LogWarning("Äîñòóï áåç email");
                    return RedirectToPage("/Account/Login");
                }

                _logger.LogInformation("Çàãðóçêà äàøáîðäà ðàáîòîäàòåëÿ äëÿ {Email}", userEmail);

                UserEmail = userEmail;
                UserFirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Ðàáîòîäàòåëü";

                LoadStatistics(userEmail);
                LoadVacanciesWithStats(userEmail);
                LoadRecentApplications(userEmail);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Îøèáêà çàãðóçêè äàøáîðäà ðàáîòîäàòåëÿ");
                return RedirectToPage("/Error");
            }
        }

        private void LoadStatistics(string userEmail)
        {
            try
            {
                // Âñåãî âàêàíñèé
                TotalVacancies = _context.Vacancies.Count(v => v.AuthorEmail == userEmail);

                // Àêòèâíûå âàêàíñèè
                ActiveVacancies = TotalVacancies;

                // Ïîëó÷àåì êîìïàíèè ðàáîòîäàòåëÿ
                var companyNames = _context.Vacancies
                    .Where(v => v.AuthorEmail == userEmail)
                    .Select(v => v.CompanyName)
                    .Distinct()
                    .ToList();

                // Âñåãî îòêëèêîâ ïî âàêàíñèÿì ýòèõ êîìïàíèé
                TotalResponses = _context.JobApplications
                    .Count(a => companyNames.Contains(a.VacancyCompanyName));

                // Íîâûå îòêëèêè (çà ïîñëåäíþþ íåäåëþ)
                var weekAgo = DateTime.Now.AddDays(-7);
                NewResponses = _context.JobApplications
                    .Count(a => companyNames.Contains(a.VacancyCompanyName) && a.AppliedAt >= weekAgo);

                _logger.LogInformation(
                    "Ñòàòèñòèêà çàãðóæåíà: Âàêàíñèé={TotalVacancies}, Îòêëèêîâ={TotalResponses}, Íîâûõ={NewResponses}",
                    TotalVacancies, TotalResponses, NewResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Îøèáêà çàãðóçêè ñòàòèñòèêè äëÿ {Email}", userEmail);
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

                _logger.LogInformation("Çàãðóæåíî {Count} âàêàíñèé ñî ñòàòèñòèêîé äëÿ {Email}",
                    Vacancies.Count, userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Îøèáêà çàãðóçêè âàêàíñèé äëÿ {Email}", userEmail);
                Vacancies = new List<VacancyWithStats>();
            }
        }

        private void LoadRecentApplications(string userEmail)
        {
            try
            {
                // Ïîëó÷àåì êîìïàíèè ðàáîòîäàòåëÿ
                var companyNames = _context.Vacancies
                    .Where(v => v.AuthorEmail == userEmail)
                    .Select(v => v.CompanyName)
                    .Distinct()
                    .ToList();

                // Ïîëó÷àåì îòêëèêè ïî âàêàíñèÿì ýòèõ êîìïàíèé
                var applications = _context.JobApplications
                    .Where(a => companyNames.Contains(a.VacancyCompanyName))
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(10)
                    .ToList();

                RecentApplications = new List<ApplicationDto>();
                foreach (var app in applications)
                {
                    // Ïîëó÷àåì èíôîðìàöèþ î ñòóäåíòå
                    var student = _context.Users.FirstOrDefault(u => u.Email == app.StudentEmail);

                    // Ïîëó÷àåì èíôîðìàöèþ î âàêàíñèè
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

                _logger.LogInformation("Çàãðóæåíî {Count} îòêëèêîâ äëÿ {Email}",
                    RecentApplications.Count, userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Îøèáêà çàãðóçêè îòêëèêîâ äëÿ {Email}", userEmail);
                RecentApplications = new List<ApplicationDto>();
            }
        }

        /// <summary>
        /// AJAX-ìåòîä äëÿ ïîëó÷åíèÿ äàííûõ ãðàôèêà
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
                _logger.LogError(ex, "Îøèáêà ïîëó÷åíèÿ äàííûõ ãðàôèêà");
                return new JsonResult(new List<object>());
            }
        }
    }
}