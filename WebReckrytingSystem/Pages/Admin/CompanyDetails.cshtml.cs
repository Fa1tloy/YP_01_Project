using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin;

[Authorize(Roles = "admin")]
public class CompanyDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CompanyDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Company? Company { get; private set; }

    public async Task<IActionResult> OnGetAsync(string name)
    {
        Company = await _context.Companies
            .AsNoTracking()
            .Include(c => c.Vacancies)
            .FirstOrDefaultAsync(c => c.Name == name);

        if (Company == null)
        {
            return NotFound();
        }

        return Page();
    }
}
