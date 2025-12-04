using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Models.DTO;

namespace WebReckrytingSystem.Services
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<User> GetAllUsers()
        {
            return _context.Users.AsNoTracking();
        }

        public IQueryable<Company> GetPendingCompanies()
        {
            return _context.Companies
                .Where(c => !c.Verified)
                .AsNoTracking();
        }

        public DashboardStats GetDashboardStats()
        {
            return new DashboardStats
            {
                TotalUsers = _context.Users.Count(),
                TotalCompanies = _context.Companies.Count(),
                TotalVacancies = _context.Vacancies.Count(),
                TotalResumes = _context.Resumes.Count(),
                PendingVerifications = _context.Companies.Count(c => !c.Verified)
            };
        }

        public void VerifyCompany(string companyName)
        {
            var company = _context.Companies.Find(companyName);
            if (company != null)
            {
                company.Verified = true;
                _context.SaveChanges();
            }
        }

        public void BlockUser(string email)
        {
            var user = _context.Users.Find(email);
            if (user != null)
            {
                user.Role = "blocked";
                _context.SaveChanges();
            }
        }
    }
}