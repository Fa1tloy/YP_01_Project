using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public interface ICompanyService
    {
        ServiceResult<Company> CreateCompany(string userEmail, CreateCompanyViewModel model);
        ICollection<Company> GetUserCompanies(string userEmail);
        Company? GetCompanyByName(string name);
    }
}