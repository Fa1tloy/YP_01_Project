using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Data
{
    public interface IVacancyRepository
    {
        Vacancy? GetByCompanyAndTitle(string companyName, string title);
        ICollection<Vacancy> GetByAuthor(string authorEmail);
        ICollection<Vacancy> GetPublishedVacancies();
        Vacancy Save(Vacancy vacancy);
        Vacancy Update(Vacancy vacancy);
        bool Delete(string companyName, string title);
    }
}