using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
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

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole != "admin" && currentUserEmail != userEmail)
            {
                if (!Resume.IsPublished)
                {
                    return NotFound();
                }
            }

            if (User.IsInRole("employer"))
            {
                try
                {
                    IsSavedByCurrentEmployer = await _context.SavedResumes
                        .AnyAsync(s => s.EmployerEmail == currentUserEmail && s.ResumeUserEmail == userEmail);
                }
                catch (MySqlException ex) when (IsSavedResumesTableMissing(ex))
                {
                    await EnsureSavedResumesTableExistsAsync();
                    IsSavedByCurrentEmployer = await _context.SavedResumes
                        .AnyAsync(s => s.EmployerEmail == currentUserEmail && s.ResumeUserEmail == userEmail);
                }

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

            SavedResume? existing;
            try
            {
                existing = await _context.SavedResumes
                    .FirstOrDefaultAsync(s => s.EmployerEmail == employerEmail && s.ResumeUserEmail == resumeUserEmail);
            }
            catch (MySqlException ex) when (IsSavedResumesTableMissing(ex))
            {
                await EnsureSavedResumesTableExistsAsync();

                existing = await _context.SavedResumes
                    .FirstOrDefaultAsync(s => s.EmployerEmail == employerEmail && s.ResumeUserEmail == resumeUserEmail);
            }

            if (existing == null)
            {
                _context.SavedResumes.Add(new SavedResume
                {
                    EmployerEmail = employerEmail,
                    ResumeUserEmail = resumeUserEmail
                });
                TempData["StatusMessage"] = "Резюме добавлено в избранное";
            }
            else
            {
                _context.SavedResumes.Remove(existing);
                TempData["StatusMessage"] = "Резюме удалено из избранного";
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (MySqlException ex) when (IsSavedResumesTableMissing(ex))
            {
                await EnsureSavedResumesTableExistsAsync();
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { userEmail = resumeUserEmail });
        }

        private static bool IsSavedResumesTableMissing(MySqlException ex)
        {
            return ex.Message.Contains("saved_resumes", StringComparison.OrdinalIgnoreCase)
                   && ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase);
        }

        private async Task EnsureSavedResumesTableExistsAsync()
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS saved_resumes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    employer_email VARCHAR(255) NOT NULL,
    resume_user_email VARCHAR(255) NOT NULL,
    saved_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_saved_resumes_unique (employer_email, resume_user_email),
    INDEX idx_saved_resumes_employer_email (employer_email),
    INDEX idx_saved_resumes_resume_user_email (resume_user_email),
    CONSTRAINT fk_saved_resumes_resume FOREIGN KEY (resume_user_email) REFERENCES resumes(user_email) ON DELETE CASCADE
);";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
