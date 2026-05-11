using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public interface ISpecialtyService
    {
        IReadOnlyList<string> GetAllNames();
        IReadOnlyList<Specialty> GetAll(); // для таблицы с Id
        void Add(string name);
        void Delete(int id);
    }
}