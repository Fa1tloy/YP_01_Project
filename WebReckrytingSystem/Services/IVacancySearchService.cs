using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public interface IVacancySearchService
    {
        ServiceResult<SearchResult<Vacancy>> SearchVacancies(SearchVacancyViewModel model);
        ServiceResult<SearchResult<Vacancy>> GetSimilarVacancies(Vacancy vacancy, int count = 5);
    }
}