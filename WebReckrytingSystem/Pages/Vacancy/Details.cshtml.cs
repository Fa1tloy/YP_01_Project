using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Vacancy
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Vacancy? Vacancy { get; private set; }
        public string? StatusMessage { get; private set; }

        public bool IsSavedByCurrentStudent { get; set; }

        public async Task<IActionResult> OnGetAsync(string companyName, string title)
        {
            Vacancy = await _context.Vacancies
                .AsNoTracking()
                .Include(v => v.Company)
                .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);

            if (Vacancy == null)
            {
                return NotFound();
            }

            if (User.Identity?.IsAuthenticated == true && User.IsInRole("job_seeker"))
            {
                var studentEmail = User.FindFirstValue(ClaimTypes.Email);
                IsSavedByCurrentStudent = await _context.SavedVacancies
                    .AnyAsync(s => s.StudentEmail == studentEmail &&
                                   s.VacancyCompanyName == companyName &&
                                   s.VacancyTitle == title);
            }

            return Page();
        }

        // Существующий обработчик для отклика
        public async Task<IActionResult> OnPostApplyAsync(string companyName, string title)
        {
            var studentEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(studentEmail) || role != "job_seeker")
            {
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Vacancy/Details", new { companyName, title }) });
            }

            Vacancy = await _context.Vacancies
                .AsNoTracking()
                .Include(v => v.Company)
                .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);

            if (Vacancy is null)
            {
                return NotFound();
            }

            var alreadyApplied = await _context.JobApplications.AnyAsync(a =>
                a.StudentEmail == studentEmail &&
                a.VacancyCompanyName == companyName &&
                a.VacancyTitle == title);

            if (alreadyApplied)
            {
                StatusMessage = "Вы уже откликались на эту вакансию.";
                return Page();
            }

            var application = new JobApplication
            {
                StudentEmail = studentEmail,
                VacancyCompanyName = companyName,
                VacancyTitle = title,
                Status = "pending",
                AppliedAt = DateTime.UtcNow
            };

            var notification = new Notification
            {
                RecipientEmail = Vacancy.AuthorEmail,
                SenderEmail = studentEmail,
                Title = "Новый отклик на вакансию",
                Message = $"Студент откликнулся на вакансию \"{Vacancy.Title}\".",
                Link = Url.Page("/Account/Chat", null, new { peer = studentEmail, companyName = Vacancy.CompanyName, title = Vacancy.Title }, Request.Scheme),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            var chatMessage = new ChatMessage
            {
                SenderEmail = studentEmail,
                RecipientEmail = Vacancy.AuthorEmail,
                Message = $"Здравствуйте! Я откликнулся(ась) на вакансию \"{Vacancy.Title}\".",
                VacancyCompanyName = Vacancy.CompanyName,
                VacancyTitle = Vacancy.Title,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.JobApplications.Add(application);
            _context.Notifications.Add(notification);
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            StatusMessage = "Отклик отправлен. Работодателю отправлено уведомление, чат создан.";
            return Page();
        }

        // Новый обработчик для добавления/удаления из избранного (для студента)
        public async Task<IActionResult> OnPostSaveVacancyAsync(string companyName, string title)
        {
            var studentEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(studentEmail) || !User.IsInRole("job_seeker"))
            {
                return RedirectToPage("/Account/Login");
            }

            // Проверяем, существует ли вакансия
            var vacancy = await _context.Vacancies
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);
            if (vacancy == null)
            {
                return NotFound();
            }

            var existing = await _context.SavedVacancies
                .FirstOrDefaultAsync(s => s.StudentEmail == studentEmail &&
                                          s.VacancyCompanyName == companyName &&
                                          s.VacancyTitle == title);

            if (existing == null)
            {
                // Добавляем в избранное
                _context.SavedVacancies.Add(new SavedVacancy
                {
                    StudentEmail = studentEmail,
                    VacancyCompanyName = companyName,
                    VacancyTitle = title
                });
                TempData["StatusMessage"] = "Вакансия добавлена в избранное";
            }
            else
            {
                // Удаляем из избранного
                _context.SavedVacancies.Remove(existing);
                TempData["StatusMessage"] = "Вакансия удалена из избранного";
            }

            await _context.SaveChangesAsync();

            // Возвращаемся на ту же страницу с деталями вакансии
            return RedirectToPage(new { companyName, title });
        }
    }
}