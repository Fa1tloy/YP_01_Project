using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
                    .Include(s => s.Vacancy) // потребуется навигационное свойство
                    .OrderByDescending(s => s.SavedAt)
                    .ToListAsync();
            }
            else if (role == "employer")
            {
                SavedResumes = await _context.SavedResumes
                    .Where(s => s.EmployerEmail == userEmail)
                    .Include(s => s.Resume) // тоже нужно навигационное свойство
                    .OrderByDescending(s => s.SavedAt)
                    .ToListAsync();
            }

            return Page();
        }
    }
}