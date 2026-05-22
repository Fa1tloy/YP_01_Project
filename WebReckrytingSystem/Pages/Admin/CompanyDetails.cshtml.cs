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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CompanyDetailsModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public Models.Company? Company { get; private set; }

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

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

    public async Task<IActionResult> OnPostUploadLogoAsync(string name)
    {
        Company = await _context.Companies
            .Include(c => c.Vacancies)
            .FirstOrDefaultAsync(c => c.Name == name);

        if (Company == null)
        {
            return NotFound();
        }

        if (LogoFile == null || LogoFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Выберите файл для загрузки.";
            return RedirectToPage(new { name });
        }

        if (string.IsNullOrWhiteSpace(LogoFile.ContentType) || !LogoFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Допускаются только изображения.";
            return RedirectToPage(new { name });
        }

        try
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "company-logos");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(LogoFile.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await LogoFile.CopyToAsync(stream);
            }

            Company.LogoUrl = $"/uploads/company-logos/{uniqueFileName}";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Логотип компании успешно обновлён.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка при загрузке логотипа: {ex.Message}";
        }

        return RedirectToPage(new { name });
    }
}