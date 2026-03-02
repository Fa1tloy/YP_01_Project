using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin;

[Authorize(Roles = "admin")]
public class ResumeDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ResumeDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Models.Resume? Resume { get; private set; }

    public async Task<IActionResult> OnGetAsync(string email)
    {
        Resume = await _context.Resumes
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserEmail == email);

        if (Resume == null)
        {
            return NotFound();
        }

        return Page();
    }
}
