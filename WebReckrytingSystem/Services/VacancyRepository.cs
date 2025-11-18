using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class VacancyRepository : IVacancyRepository
    {
        private readonly ApplicationDbContext _context;

        public VacancyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Vacancy? GetByCompanyAndTitle(string companyName, string title)
        {
            return _context.Vacancies
                .Include(v => v.Company)
                .Include(v => v.Author)
                .FirstOrDefault(v => v.CompanyName == companyName && v.Title == title);
        }

        public ICollection<Vacancy> GetByAuthor(string authorEmail)
        {
            return _context.Vacancies
                .Include(v => v.Company)
                .Where(v => v.AuthorEmail == authorEmail)
                .OrderByDescending(v => v.CompanyName) // Сортируем по названию компании
                .ToList();
        }

        public ICollection<Vacancy> GetPublishedVacancies()
        {
            return _context.Vacancies
                .Include(v => v.Company)
                .Include(v => v.Author)
                .OrderByDescending(v => v.CompanyName)
                .ToList();
        }

        public Vacancy Save(Vacancy vacancy)
        {
            _context.Vacancies.Add(vacancy);
            _context.SaveChanges();
            return vacancy;
        }

        public Vacancy Update(Vacancy vacancy)
        {
            _context.Vacancies.Update(vacancy);
            _context.SaveChanges();
            return vacancy;
        }

        public bool Delete(string companyName, string title)
        {
            var vacancy = GetByCompanyAndTitle(companyName, title);
            if (vacancy != null)
            {
                _context.Vacancies.Remove(vacancy);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}