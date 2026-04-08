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
                catch (MySqlException ex) when (IsSavedResumesTableMissing(ex))
                {
                    await EnsureSavedResumesTableExistsAsync();
                    SavedResumes = new List<SavedResume>();
                }
            }

            return Page();
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
