using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;

        public CompanyService(ICompanyRepository companyRepository, IUserRepository userRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public ServiceResult<Company> CreateCompany(string userEmail, CreateCompanyViewModel model)
        {
            // Проверка прав доступа
            var user = _userRepository.FindByEmail(userEmail);
            if (user == null || user.Role != "employer")
                return ServiceResult<Company>.Error("Только работодатели могут создавать компании");

            // Валидация данных
            if (string.IsNullOrWhiteSpace(model.Name))
                return ServiceResult<Company>.Error("Название компании обязательно");

            // Проверка на существующую компанию
            var existingCompany = _companyRepository.FindByName(model.Name.Trim());
            if (existingCompany != null)
                return ServiceResult<Company>.Error("Компания с таким названием уже существует");

            try
            {
                var company = new Company
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    Website = model.Website?.Trim(),
                    Verified = false // Новые компании не верифицированы
                };

                var savedCompany = _companyRepository.Save(company);
                return ServiceResult<Company>.Success("Компания успешно создана!", savedCompany);
            }
            catch (Exception ex)
            {
                return ServiceResult<Company>.Error($"Ошибка при создании компании: {ex.Message}");
            }
        }

        public ICollection<Company> GetUserCompanies(string userEmail)
        {
            return _companyRepository.GetUserCompanies(userEmail);
        }

        public Company? GetCompanyByName(string name)
        {
            return _companyRepository.FindByName(name);
        }
    }
}