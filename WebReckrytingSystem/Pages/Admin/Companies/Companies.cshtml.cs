using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Admin.Companies
{
    [Authorize(Roles = "admin")]
    public class CompaniesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompanyRepository _companyRepository;

        public CompaniesModel(ApplicationDbContext context, ICompanyRepository companyRepository)
        {
            _context = context;
            _companyRepository = companyRepository;
        }

        public SearchResult<Models.Company> Companies { get; set; } = new();

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

        public async Task<IActionResult> OnPostVerifyAsync(string companyName)
        {
            var company = await _context.Companies.FindAsync(companyName);
            if (company != null)
            {
                company.Verified = true;
                
                // Verify all employer users of this company
                var employerUsers = _context.Users
                    .Where(u => u.CompanyName == companyName && u.Role == User.ROLE_EMPLOYER)
                    .ToList();
                
                foreach (var user in employerUsers)
                {
                    user.IsVerified = true;
                }
                
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
