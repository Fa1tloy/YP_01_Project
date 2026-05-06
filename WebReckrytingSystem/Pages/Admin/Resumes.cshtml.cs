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
    public class ResumesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResumesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SearchResult<Models.Resume> Resumes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchEmail { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchPosition { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public void OnGet()
        {
            var query = _context.Resumes
                .Include(r => r.User)
                .AsQueryable();

                .Skip((PageNumber - 1) * PageSize)
                PageNumber = this.PageNumber,
                query = query.Where(r => r.UserEmail.Contains(SearchEmail));

            if (!string.IsNullOrWhiteSpace(SearchPosition))
                query = query.Where(r => r.DesiredPosition.Contains(SearchPosition));

            if (FilterStatus == "published")
                query = query.Where(r => r.IsPublished);
            else if (FilterStatus == "draft")
                query = query.Where(r => !r.IsPublished);

            var totalCount = query.Count();

            var items = query
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Resumes = new SearchResult<Models.Resume>
            {
                Items = items,
                TotalCount = totalCount,
                Page = Page,
                PageSize = PageSize
            };
        }

        public async Task<IActionResult> OnPostDeleteAsync(string email)
        {
            var resume = await _context.Resumes.FindAsync(email);
            if (resume != null)
            {
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ðåçþìå óñïåøíî óäàëåíî";
            }
            else
            {
                TempData["ErrorMessage"] = "Ðåçþìå íå íàéäåíî";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTogglePublishAsync(string email)
        {
            var resume = await _context.Resumes.FindAsync(email);
            if (resume != null)
            {
                resume.IsPublished = !resume.IsPublished;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = resume.IsPublished ? "Ðåçþìå îïóáëèêîâàíî" : "Ðåçþìå ñíÿòî ñ ïóáëèêàöèè";
            }
            return RedirectToPage();
        }
    }
}