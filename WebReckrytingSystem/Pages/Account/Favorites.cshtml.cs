using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Security.Claims;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize]
    public class FavoritesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FavoritesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<SavedVacancy> SavedVacancies { get; set; } = new();
        public List<SavedResume> SavedResumes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "job_seeker")
            {
                SavedVacancies = await _context.SavedVacancies
                    .Where(s => s.StudentEmail == userEmail)
                    .Include(s => s.Vacancy)
                    .OrderByDescending(s => s.SavedAt)
                    .ToListAsync();
            }
            else if (role == "employer")
            {
                try
                {
                    SavedResumes = await _context.SavedResumes
                        .Where(s => s.EmployerEmail == userEmail)
                        .Include(s => s.Resume)
                        .OrderByDescending(s => s.SavedAt)
                        .ToListAsync();
                }
                catch (MySqlException ex) when (IsSavedResumesSchemaIssue(ex))
                {
                    await EnsureSavedResumesTableExistsAsync();

                    SavedResumes = await _context.SavedResumes
                        .Where(s => s.EmployerEmail == userEmail)
                        .Include(s => s.Resume)
                        .OrderByDescending(s => s.SavedAt)
                        .ToListAsync();
                }
            }

            return Page();
        }

        // Удаление сохранённой вакансии
        public async Task<IActionResult> OnPostDeleteVacancyAsync(int id)
        {
            var saved = await _context.SavedVacancies.FindAsync(id);
            if (saved != null)
            {
                _context.SavedVacancies.Remove(saved);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Вакансия удалена из избранного";
            }
            return RedirectToPage();
        }

        // Удаление сохранённого резюме
        public async Task<IActionResult> OnPostDeleteResumeAsync(int id)
        {
            var saved = await _context.SavedResumes.FindAsync(id);
            if (saved != null)
            {
                _context.SavedResumes.Remove(saved);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Резюме удалено из избранного";
            }
            return RedirectToPage();
        }

        private static bool IsSavedResumesSchemaIssue(MySqlException ex)
        {
            var isTableMissing = ex.Message.Contains("saved_resumes", StringComparison.OrdinalIgnoreCase)
                                 && ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase);
            var isCollationMismatch = ex.Message.Contains("Illegal mix of collations", StringComparison.OrdinalIgnoreCase);
            return isTableMissing || isCollationMismatch;
        }

        private async Task EnsureSavedResumesTableExistsAsync()
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS saved_resumes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    employer_email VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    resume_user_email VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    saved_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_saved_resumes_unique (employer_email, resume_user_email),
    INDEX idx_saved_resumes_employer_email (employer_email),
    INDEX idx_saved_resumes_resume_user_email (resume_user_email)
)
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;";

            const string alterSql = @"
ALTER TABLE saved_resumes
    CONVERT TO CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;";

            await _context.Database.ExecuteSqlRawAsync(sql);
            await _context.Database.ExecuteSqlRawAsync(alterSql);
        }
    }
}