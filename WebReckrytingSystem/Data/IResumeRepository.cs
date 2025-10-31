// Data/IResumeRepository.cs
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Data
{
    public interface IResumeRepository
    {
        Resume? GetByUserEmail(string userEmail);
        Resume Save(Resume resume);
        Resume Update(Resume resume);
        bool Delete(string userEmail);
    }
}

