using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Account
{
    [Authorize]
    public class NotificationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public NotificationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Notification> Items { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            Items = await _context.Notifications
                .Where(n => n.RecipientEmail == userEmail)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();

            var unread = Items.Where(x => !x.IsRead).ToList();
            if (unread.Count > 0)
            {
                unread.ForEach(x => x.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return Page();
        }
    }
}
