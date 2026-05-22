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
    private readonly ILogger<EditModel> _logger;

    public EditModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<EditModel> logger)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
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

    public async Task<IActionResult> OnPostAsync(string? name)
    {
        var companyName = string.IsNullOrWhiteSpace(name) ? Company.Name : name;
        if (string.IsNullOrWhiteSpace(companyName))
            return NotFound();

        var companyFromDb = await _context.Companies.FindAsync(companyName);
        if (companyFromDb == null)
            return NotFound();

        ModelState.Remove("Company.Name");
        if (!ModelState.IsValid)
        {
            Company = companyFromDb;
            return Page();
        }

        // Название компании не меняем (Primary Key)
        companyFromDb.Description = Company.Description?.Trim();
        companyFromDb.Website = Company.Website?.Trim();
        companyFromDb.Verified = Company.Verified;

        if (LogoFile is { Length: > 0 })
        {
            if (string.IsNullOrWhiteSpace(LogoFile.ContentType) ||
                !LogoFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(LogoFile), "Допускаются только изображения.");
                return Page();
            }

            try
            {
                var webRootPath = ResolveWebRootPath();
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "company-logos");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(LogoFile.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".png";
                }
                var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using var stream = System.IO.File.Create(filePath);
                await LogoFile.CopyToAsync(stream);

                companyFromDb.LogoUrl = $"/uploads/company-logos/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки логотипа компании {CompanyName}", companyFromDb.Name);
                ModelState.AddModelError(nameof(LogoFile), "Не удалось загрузить логотип. Попробуйте ещё раз.");
                Company = companyFromDb;
                return Page();
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Компания успешно обновлена.";
        return RedirectToPage("/Admin/Companies/Companies");
    }

    private string ResolveWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath))
        {
            return _webHostEnvironment.WebRootPath;
        }

        var fallback = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        Directory.CreateDirectory(fallback);
        return fallback;
    }
}
