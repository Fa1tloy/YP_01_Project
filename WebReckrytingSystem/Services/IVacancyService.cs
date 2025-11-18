using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public interface IVacancyService
    {
        ServiceResult<Vacancy> CreateVacancy(string authorEmail, CreateVacancyViewModel model);
        ServiceResult<Vacancy> UpdateVacancy(string companyName, string title, CreateVacancyViewModel model);
        Vacancy? GetVacancy(string companyName, string title);
        ICollection<Vacancy> GetUserVacancies(string authorEmail);
        ICollection<Vacancy> GetAllVacancies();
    }
}