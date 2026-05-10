using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin.Companies
{
    [Authorize(Roles = "admin")]
    public class CompaniesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompaniesModel> _logger;

        public CompaniesModel(ApplicationDbContext context, ILogger<CompaniesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public SearchResult<Models.Company> Companies { get; set; } = new();

        public Dictionary<string, int> VacancyCounts { get; set; } = new();

        public string? ErrorMessage { get; set; }

        [BindProperty]
        public CreateCompanyViewModel CreateCompany { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public void OnGet()
        {
            LoadCompanies();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            NormalizeCreateCompanyInput();

            ValidateWebsite();

            if (!ModelState.IsValid)
            {
                LoadCompanies();
                return Page();
            }

            var companyName = CreateCompany.Name.Trim();
            var existingCompany = await _context.Companies.FindAsync(companyName);
            if (existingCompany != null)
            {
                ModelState.AddModelError("CreateCompany.Name", "Компания с таким названием уже существует");
                LoadCompanies();
                return Page();
            }

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания компании {CompanyName}", companyName);
                ErrorMessage = "Не удалось создать компанию. Проверьте данные и попробуйте ещё раз.";
                ModelState.AddModelError(string.Empty, ErrorMessage);
                LoadCompanies();
                return Page();
            }
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

        public int GetVacancyCount(string companyName)
        {
            return VacancyCounts.TryGetValue(companyName, out var count) ? count : 0;
        }

        private void LoadCompanies()
        {
            var query = _context.Companies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchName))
                query = query.Where(c => c.Name.Contains(SearchName));

            if (FilterStatus == "verified")
                query = query.Where(c => c.Verified);
            else if (FilterStatus == "pending")
                query = query.Where(c => !c.Verified);

            var totalCount = query.Count();

            var items = query
                .OrderBy(c => c.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Companies = new SearchResult<Models.Company>
            {
                Items = items,
                TotalCount = totalCount,
                Page = PageNumber,
                PageSize = PageSize
            };

            try
            {
                var companyNames = items.Select(c => c.Name).ToList();
                VacancyCounts = _context.Vacancies
                    .Where(v => companyNames.Contains(v.CompanyName))
                    .GroupBy(v => v.CompanyName)
                    .Select(g => new { CompanyName = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.CompanyName, x => x.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки количества вакансий по компаниям");
                VacancyCounts = new Dictionary<string, int>();
                ErrorMessage ??= "Компании загружены, но не удалось получить количество вакансий.";
            }
        }

        private void NormalizeCreateCompanyInput()
        {
            CreateCompany.Name = CreateCompany.Name.Trim();
            CreateCompany.Description = string.IsNullOrWhiteSpace(CreateCompany.Description)
                ? null
                : CreateCompany.Description.Trim();
            CreateCompany.Website = string.IsNullOrWhiteSpace(CreateCompany.Website)
                ? null
                : CreateCompany.Website.Trim();

            if (!string.IsNullOrWhiteSpace(CreateCompany.Website)
                && !CreateCompany.Website.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !CreateCompany.Website.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                CreateCompany.Website = $"https://{CreateCompany.Website}";
            }
        }

        private void ValidateWebsite()
        {
            if (string.IsNullOrWhiteSpace(CreateCompany.Website))
            {
                return;
            }

            if (!Uri.TryCreate(CreateCompany.Website, UriKind.Absolute, out var websiteUri)
                || string.IsNullOrWhiteSpace(websiteUri.Host)
                || (websiteUri.Scheme != Uri.UriSchemeHttp && websiteUri.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("CreateCompany.Website", "Введите корректный сайт компании, например example.com или https://example.com");
            }
        }
    }
}
