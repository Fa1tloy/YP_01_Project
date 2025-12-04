using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Company? FindByName(string name)
        {
            return _context.Companies.FirstOrDefault(c => c.Name == name);
        }

        public ICollection<Company> GetUserCompanies(string userEmail)
        {
            return _context.Vacancies
                .Where(v => v.AuthorEmail == userEmail)
                .Select(v => v.Company)
                .Distinct()
                .ToList();
        }

        public Company Save(Company company)
        {
            _context.Companies.Add(company);
            _context.SaveChanges();
            return company;
        }

        public Company Update(Company company)
        {
            _context.Companies.Update(company);
            _context.SaveChanges();
            return company;
        }
    }
}