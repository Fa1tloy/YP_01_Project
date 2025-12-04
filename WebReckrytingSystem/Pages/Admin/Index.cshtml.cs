using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public IndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public Models.DTO.DashboardStats Stats { get; set; } = new();

        public void OnGet()
        {
            Stats = _adminService.GetStats();
        }
    }
}