// Services/ResumeRepository.cs
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class ResumeRepository : IResumeRepository
    {
        private readonly ApplicationDbContext _context;

        public ResumeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Resume? GetByUserEmail(string userEmail)
        {
            return _context.Resumes.FirstOrDefault(r => r.UserEmail == userEmail);
        }

        public Resume Save(Resume resume)
        {
            _context.Resumes.Add(resume);
            _context.SaveChanges();
            return resume;
        }

        public Resume Update(Resume resume)
        {
            _context.Resumes.Update(resume);
            _context.SaveChanges();
            return resume;
        }

        public bool Delete(string userEmail)
        {
            var resume = GetByUserEmail(userEmail);
            if (resume != null)
            {
                _context.Resumes.Remove(resume);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}