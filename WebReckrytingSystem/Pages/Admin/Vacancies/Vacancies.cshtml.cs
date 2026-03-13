using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class VacanciesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public VacanciesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SearchResult<Models.Vacancy> Vacancies { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTitle { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchCompany { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public void OnGet()
        {
            var query = _context.Vacancies
                .Include(v => v.Company)
                .Include(v => v.Author)
                .AsQueryable();

                .Skip((PageNumber - 1) * PageSize)
                PageNumber = this.PageNumber,
                query = query.Where(v => v.Title.Contains(SearchTitle));

            if (!string.IsNullOrWhiteSpace(SearchCompany))
                query = query.Where(v => v.CompanyName.Contains(SearchCompany));

            var totalCount = query.Count();

            var items = query
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Vacancies = new SearchResult<Models.Vacancy>
            {
                Items = items,
                TotalCount = totalCount,
                Page = Page,
                PageSize = PageSize
            };
        }

        public async Task<IActionResult> OnPostDeleteAsync(string companyName, string title)
        {
            var vacancy = await _context.Vacancies.FindAsync(companyName, title);
            if (vacancy != null)
            {
                _context.Vacancies.Remove(vacancy);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}