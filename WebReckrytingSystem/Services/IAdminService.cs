using System.Collections.Generic;
using System.Threading.Tasks;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Models.DTO;

namespace WebReckrytingSystem.Services
{
    public interface IAdminService
    {
        DashboardStats GetStats();
        Task<List<User>> GetUsersAsync();
        Task<List<Company>> GetPendingCompaniesAsync();
        Task<bool> VerifyCompanyAsync(string companyName);
        Task<bool> BlockUserAsync(string email);
    }
}