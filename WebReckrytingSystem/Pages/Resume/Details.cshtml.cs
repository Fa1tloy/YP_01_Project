using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Resume
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Resume? Resume { get; private set; }
        public bool IsSavedByCurrentEmployer { get; set; }
        public bool HasExistingChat { get; set; }

        public async Task<IActionResult> OnGetAsync(string userEmail)
        {
            Resume = await _context.Resumes
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserEmail == userEmail);

            if (Resume == null)
            {
                return NotFound();
            }

            // Проверяем права доступа
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Если пользователь не админ и не владелец резюме, проверяем публикацию
            if (currentUserRole != "admin" && currentUserEmail != userEmail)
            {
                if (!Resume.IsPublished)
                {
                    return NotFound();
                }
            }

            // Проверяем, сохранено ли резюме в избранное (для работодателя)
            if (User.IsInRole("employer"))
            {
                IsSavedByCurrentEmployer = await _context.SavedResumes
                    .AnyAsync(s => s.EmployerEmail == currentUserEmail && s.ResumeUserEmail == userEmail);

                // Проверяем, есть ли уже чат
                HasExistingChat = await _context.ChatMessages
                    .AnyAsync(m => (m.SenderEmail == currentUserEmail && m.RecipientEmail == userEmail) ||
                                   (m.SenderEmail == userEmail && m.RecipientEmail == currentUserEmail));
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveResumeAsync(string resumeUserEmail)
        {
            var employerEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(employerEmail) || !User.IsInRole("employer"))
            {
                return RedirectToPage("/Account/Login");
            }

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.UserEmail == resumeUserEmail);
            if (resume == null)
            {
                return NotFound();
            }

            var existing = await _context.SavedResumes
                .FirstOrDefaultAsync(s => s.EmployerEmail == employerEmail && s.ResumeUserEmail == resumeUserEmail);

            if (existing == null)
            {
                _context.SavedResumes.Add(new SavedResume
                {
                    EmployerEmail = employerEmail,
                    ResumeUserEmail = resumeUserEmail
                });
                TempData["StatusMessage"] = "? Резюме добавлено в избранное";
            }
            else
            {
                _context.SavedResumes.Remove(existing);
                TempData["StatusMessage"] = "??? Резюме удалено из избранного";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { userEmail = resumeUserEmail });
        }
    }
}