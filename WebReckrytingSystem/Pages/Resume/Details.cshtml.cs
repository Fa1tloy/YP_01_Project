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

        // Новое свойство для хранения информации о том, сохранено ли резюме текущим работодателем
        public bool IsSavedByCurrentEmployer { get; set; }

        public async Task<IActionResult> OnGetAsync(string userEmail)
        {
            Resume = await _context.Resumes
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserEmail == userEmail && r.IsPublished);

            if (Resume == null)
            {
                return NotFound();
            }

            // Проверяем, сохранено ли это резюме в избранное у текущего пользователя (если он работодатель)
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("employer"))
            {
                var employerEmail = User.FindFirstValue(ClaimTypes.Email);
                IsSavedByCurrentEmployer = await _context.SavedResumes
                    .AnyAsync(s => s.EmployerEmail == employerEmail && s.ResumeUserEmail == userEmail);
            }

            return Page();
        }

        // Обработчик для добавления/удаления резюме в избранное
        public async Task<IActionResult> OnPostSaveResumeAsync(string resumeUserEmail)
        {
            var employerEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(employerEmail) || !User.IsInRole("employer"))
            {
                return RedirectToPage("/Account/Login");
            }

            var resume = await _context.Resumes
                .AsNoTracking()
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
                TempData["StatusMessage"] = "Резюме добавлено в избранное";
            }
            else
            {
                _context.SavedResumes.Remove(existing);
                TempData["StatusMessage"] = "Резюме удалено из избранного";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { userEmail = resumeUserEmail });
        }
    }
}