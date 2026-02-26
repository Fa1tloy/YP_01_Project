using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Resume
{
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SearchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public List<Models.Resume> Resumes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _context.Resumes
                .AsNoTracking()
                .Where(r => r.IsPublished);

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var term = SearchQuery.Trim();
                query = query.Where(r =>
                    r.DesiredPosition.Contains(term) ||
                    (r.Skills != null && r.Skills.Contains(term)) ||
                    r.UserEmail.Contains(term));
            }

            Resumes = await query
                .OrderBy(r => r.DesiredPosition)
                .Take(50)
                .ToListAsync();
        }
    }
}
