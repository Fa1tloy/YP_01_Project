using System.Linq;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Data
{
    public interface IAdminRepository
    {
        IQueryable<User> GetAllUsers();
        IQueryable<Company> GetPendingCompanies();
        Models.DTO.DashboardStats GetDashboardStats();
        void VerifyCompany(string companyName);
        void BlockUser(string email);
    }
}