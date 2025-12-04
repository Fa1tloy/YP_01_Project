using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Models.DTO;

namespace WebReckrytingSystem.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _repo;

        public AdminService(IAdminRepository repo)
        {
            _repo = repo;
        }

        public DashboardStats GetStats() => _repo.GetDashboardStats();

        public async Task<List<User>> GetUsersAsync()
        {
            return await _repo.GetAllUsers().ToListAsync();
        }

        public async Task<List<Company>> GetPendingCompaniesAsync()
        {
            return await _repo.GetPendingCompanies().ToListAsync();
        }

        public async Task<bool> VerifyCompanyAsync(string companyName)
        {
            try
            {
                _repo.VerifyCompany(companyName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BlockUserAsync(string email)
        {
            try
            {
                _repo.BlockUser(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}