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
        public bool HasExistingChat { get; set; }

        public async Task<IActionResult> OnGetAsync(string companyName, string title)
        {
            Vacancy = await _context.Vacancies
                .AsNoTracking()
                .Include(v => v.Company)
                .Include(v => v.Author)
                .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);

            if (Vacancy == null)
                return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);

                if (User.IsInRole("job_seeker"))
                {
                    // Проверяем, сохранена ли вакансия
                    IsSavedByCurrentStudent = await _context.SavedVacancies
                        .AnyAsync(s => s.StudentEmail == userEmail &&
                                       s.VacancyCompanyName == companyName &&
                                       s.VacancyTitle == title);

                    // Проверяем, есть ли уже чат с автором ИМЕННО ПО ЭТОЙ ВАКАНСИИ
                    HasExistingChat = await _context.ChatMessages
                        .AnyAsync(m =>
                            ((m.SenderEmail == userEmail && m.RecipientEmail == Vacancy.AuthorEmail) ||
                             (m.SenderEmail == Vacancy.AuthorEmail && m.RecipientEmail == userEmail)) &&
                            m.VacancyCompanyName == companyName &&
                            m.VacancyTitle == title);
                }
            }

            // Показываем сообщение из TempData, если есть
            StatusMessage = TempData["StatusMessage"]?.ToString();
            return Page();
        }

        public async Task<IActionResult> OnPostApplyAsync(string companyName, string title)
        {
            var studentEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(studentEmail) || role != "job_seeker")
                return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("/Vacancy/Details", new { companyName, title }) });

            Vacancy = await _context.Vacancies
                .AsNoTracking()
                .Include(v => v.Company)
                .Include(v => v.Author)
                .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);

            if (Vacancy == null)
                return NotFound();

            // Удаляем старый отклик, если он существует (чтобы можно было переоткликнуться после удаления чата)
            var existingApp = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.StudentEmail == studentEmail &&
                                          a.VacancyCompanyName == companyName &&
                                          a.VacancyTitle == title);
            if (existingApp != null)
            {
                _context.JobApplications.Remove(existingApp);
            }

            // Создаём новый отклик
            _context.JobApplications.Add(new JobApplication
            {
                StudentEmail = studentEmail,
                VacancyCompanyName = companyName,
                VacancyTitle = title,
                Status = "pending",
                AppliedAt = DateTime.UtcNow
            });

            // Уведомление автору (администратору)
            _context.Notifications.Add(new Notification
            {
                RecipientEmail = Vacancy.AuthorEmail,
                SenderEmail = studentEmail,
                Title = "Новый отклик на вакансию",
                Message = $"Студент откликнулся на вакансию \"{Vacancy.Title}\"",
                Link = Url.Page("/Account/Chat", null, new { peer = studentEmail, companyName = Vacancy.CompanyName, title = Vacancy.Title }, Request.Scheme),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });

            // Первое сообщение в чат
            _context.ChatMessages.Add(new ChatMessage
            {
                SenderEmail = studentEmail,
                RecipientEmail = Vacancy.AuthorEmail,
                Message = $"Здравствуйте! Меня заинтересовала вакансия \"{Vacancy.Title}\".",
                VacancyCompanyName = Vacancy.CompanyName,
                VacancyTitle = Vacancy.Title,
                SentAt = DateTime.UtcNow,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Отклик отправлен! Теперь вы можете общаться в чате.";
            return RedirectToPage("/Account/Chat", new { peer = Vacancy.AuthorEmail, companyName = Vacancy.CompanyName, title = Vacancy.Title });
        }

        public async Task<IActionResult> OnPostSaveVacancyAsync(string companyName, string title)
        {
            var studentEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(studentEmail) || !User.IsInRole("job_seeker"))
                return RedirectToPage("/Account/Login");

            var vacancy = await _context.Vacancies
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);
            if (vacancy == null)
                return NotFound();

            var existing = await _context.SavedVacancies
                .FirstOrDefaultAsync(s => s.StudentEmail == studentEmail &&
                                          s.VacancyCompanyName == companyName &&
                                          s.VacancyTitle == title);

            if (existing == null)
            {
                _context.SavedVacancies.Add(new SavedVacancy
                {
                    StudentEmail = studentEmail,
                    VacancyCompanyName = companyName,
                    VacancyTitle = title,
                    SavedAt = DateTime.UtcNow
                });
                TempData["StatusMessage"] = "✅ Вакансия добавлена в избранное";
            }
            else
            {
                _context.SavedVacancies.Remove(existing);
                TempData["StatusMessage"] = "🗑️ Вакансия удалена из избранного";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { companyName, title });
        }
    }
}