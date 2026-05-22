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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EditModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    [BindProperty]
    public Company Company { get; set; } = new();

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

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

        if (LogoFile is { Length: > 0 })
        {
            if (!LogoFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(LogoFile), "Допускаются только изображения.");
                return Page();
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "company-logos");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(LogoFile.FileName);
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var stream = System.IO.File.Create(filePath);
            await LogoFile.CopyToAsync(stream);

            companyFromDb.LogoUrl = $"/uploads/company-logos/{uniqueFileName}";
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Компания успешно обновлена.";
        return RedirectToPage("/Admin/Companies/Companies");
    }
}