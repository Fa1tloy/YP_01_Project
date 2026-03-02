using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin;

[Authorize(Roles = "admin")]
public class UserDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public UserDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public User? UserData { get; private set; }

    public async Task<IActionResult> OnGetAsync(string email)
    {
        UserData = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        if (UserData == null)
        {
            return NotFound();
        }

        return Page();
    }
}
