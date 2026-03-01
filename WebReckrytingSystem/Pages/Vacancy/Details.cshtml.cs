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

        public async Task<IActionResult> OnGetAsync(string companyName, string title)
        {
            Vacancy = await _context.Vacancies
                .AsNoTracking()
                .Include(v => v.Company)
                .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);

            return Page();
        }

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
    }
}
