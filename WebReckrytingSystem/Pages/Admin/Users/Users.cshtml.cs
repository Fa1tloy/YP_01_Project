using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin.Users
{
    [Authorize(Roles = "admin")]
    public class UsersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UsersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SearchResult<User> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchEmail { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterRole { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public void OnGet()
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(SearchEmail))
                query = query.Where(u => u.Email.Contains(SearchEmail));

            if (!string.IsNullOrWhiteSpace(FilterRole))
                query = query.Where(u => u.Role == FilterRole);

            // Total count
            var totalCount = query.Count();

            // Pagination
            var items = query
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Users = new SearchResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                Page = Page,
                PageSize = PageSize
            };
        }

        public async Task<IActionResult> OnPostToggleBlockAsync(string email)
        {
            var user = await _context.Users.FindAsync(email);
            if (user == null)
                return NotFound();

            if (user.Role == "admin")
            {
                TempData["ErrorMessage"] = "Администратора нельзя блокировать.";
                return RedirectToPage();
            }

            if (user.Role == "blocked")
            {
                user.Role = string.IsNullOrWhiteSpace(user.CompanyName) ? "job_seeker" : "employer";
            }
            else
            {
                user.Role = "blocked";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}