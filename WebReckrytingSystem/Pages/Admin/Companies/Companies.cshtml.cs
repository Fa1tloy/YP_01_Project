using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin.Companies
{
    [Authorize(Roles = "admin")]
    public class CompaniesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CompaniesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SearchResult<Models.Company> Companies { get; set; } = new();

        [BindProperty]
        public CreateCompanyViewModel CreateCompany { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public void OnGet()
        {
            var query = _context.Companies
                .Include(c => c.Vacancies)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchName))
                query = query.Where(c => c.Name.Contains(SearchName));

            if (FilterStatus == "verified")
                query = query.Where(c => c.Verified);
            else if (FilterStatus == "pending")
                query = query.Where(c => !c.Verified);

            var totalCount = query.Count();

            var items = query
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Companies = new SearchResult<Models.Company>
            {
                Items = items,
                TotalCount = totalCount,
                Page = Page,
                PageSize = PageSize
            };
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            var companyName = CreateCompany.Name.Trim();
            var existingCompany = await _context.Companies.FindAsync(companyName);
            if (existingCompany != null)
            {
                ModelState.AddModelError("CreateCompany.Name", "Компания с таким названием уже существует");
                OnGet();
                return Page();
            }

            _context.Companies.Add(new Models.Company
            {
                Name = companyName,
                Description = CreateCompany.Description?.Trim(),
                Website = CreateCompany.Website?.Trim(),
                Verified = true
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Компания успешно создана";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostVerifyAsync(string companyName)
        {
            var company = await _context.Companies.FindAsync(companyName);
            if (company != null)
            {
                company.Verified = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string companyName)
        {
            var company = await _context.Companies.FindAsync(companyName);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
