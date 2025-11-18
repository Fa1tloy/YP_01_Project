using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Data
{
    public interface ICompanyRepository
    {
        Company? FindByName(string name);
        ICollection<Company> GetUserCompanies(string userEmail);
        Company Save(Company company);
        Company Update(Company company);
    }
}