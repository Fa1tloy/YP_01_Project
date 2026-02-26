using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Resume
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Resume? Resume { get; private set; }

        public async Task<IActionResult> OnGetAsync(string userEmail)
        {
            Resume = await _context.Resumes
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserEmail == userEmail && r.IsPublished);

            return Page();
        }
    }
}
