using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin.Companies;

[Authorize(Roles = "admin")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Company Company { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string name)
    {
        Company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name);

        if (Company == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string name)
    {
        if (!ModelState.IsValid)
            return Page();

        var companyFromDb = await _context.Companies.FindAsync(name);
        if (companyFromDb == null)
            return NotFound();

        // Название компании не меняем (Primary Key)
        companyFromDb.Description = Company.Description?.Trim();
        companyFromDb.Website = Company.Website?.Trim();
        companyFromDb.Verified = Company.Verified;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Компания успешно обновлена.";
        return RedirectToPage("/Admin/Companies/Companies");
    }
}