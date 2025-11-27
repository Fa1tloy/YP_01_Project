using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    // IVacancyService.cs
    public interface IVacancyService
    {
        ServiceResult<Vacancy> CreateVacancy(string authorEmail, CreateVacancyViewModel model);
        ServiceResult<Vacancy> UpdateVacancy(string companyName, string title, string userEmail, CreateVacancyViewModel model);
        Vacancy? GetVacancy(string companyName, string title);
        ICollection<Vacancy> GetUserVacancies(string authorEmail);
        ICollection<Vacancy> GetAllVacancies();
    }
}