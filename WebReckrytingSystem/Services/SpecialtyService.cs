using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly ApplicationDbContext _context;

        public SpecialtyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<string> GetAllNames()
        {
            return _context.Specialties
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => s.Name)
                .ToList();
        }

        public IReadOnlyList<Specialty> GetAll()
        {
            return _context.Specialties
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToList();
        }

        public void Add(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название специальности не может быть пустым");

            name = name.Trim();
            if (_context.Specialties.Any(s => s.Name == name))
                throw new InvalidOperationException("Такая специальность уже существует");

            _context.Specialties.Add(new Specialty { Name = name });
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Specialties.Find(id);
            if (entity != null)
            {
                _context.Specialties.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}