using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin;

[Authorize(Roles = "admin")]
public class VacancyDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public VacancyDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Models.Vacancy? Vacancy { get; private set; }

    public async Task<IActionResult> OnGetAsync(string companyName, string title)
    {
        Vacancy = await _context.Vacancies
            .AsNoTracking()
            .Include(v => v.Company)
            .Include(v => v.Author)
            .FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);

        if (Vacancy == null)
        {
            return NotFound();
        }

        return Page();
    }
}
