using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class VacancyService : IVacancyService
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;

        public VacancyService(IVacancyRepository vacancyRepository,
                            ICompanyRepository companyRepository,
                            IUserRepository userRepository)
        {
            _vacancyRepository = vacancyRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public ServiceResult<Vacancy> CreateVacancy(string authorEmail, CreateVacancyViewModel model)
        {
            // 1. Проверка прав доступа
            var user = _userRepository.FindByEmail(authorEmail);
            if (user == null || user.Role != "employer")
                return ServiceResult<Vacancy>.Error("Только работодатели могут создавать вакансии");

            // 2. Валидация компании
            var company = _companyRepository.FindByName(model.CompanyName);
            if (company == null)
                return ServiceResult<Vacancy>.Error("Компания не найдена");

            // 3. Валидация данных вакансии
            var validationResult = ValidateVacancyData(model);
            if (!validationResult.IsSuccess)
                return ServiceResult<Vacancy>.Error(validationResult.Message);

            // 4. Проверка дубликатов
            var existingVacancy = _vacancyRepository.GetByCompanyAndTitle(model.CompanyName, model.Title);
            if (existingVacancy != null)
                return ServiceResult<Vacancy>.Error("Вакансия с таким названием уже существует в этой компании");

            try
            {
                // 5. Создание вакансии
                var vacancy = new Vacancy
                {
                    CompanyName = model.CompanyName.Trim(),
                    Title = model.Title.Trim(),
                    Description = model.Description.Trim(),
                    Requirements = model.Requirements.Trim(),
                    SalaryFrom = model.SalaryFrom,
                    SalaryTo = model.SalaryTo,
                    EmploymentType = model.EmploymentType,
                    WorkSchedule = model.WorkSchedule,
                    AuthorEmail = authorEmail
                };

                var savedVacancy = _vacancyRepository.Save(vacancy);
                return ServiceResult<Vacancy>.Success("Вакансия успешно опубликована!", savedVacancy);
            }
            catch (Exception ex)
            {
                return ServiceResult<Vacancy>.Error($"Ошибка при создании вакансии: {ex.Message}");
            }
        }

        private ServiceResult ValidateVacancyData(CreateVacancyViewModel model)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(model.Title))
                return ServiceResult.Error("Название вакансии обязательно");

            if (string.IsNullOrWhiteSpace(model.Description))
                return ServiceResult.Error("Описание вакансии обязательно");

            if (string.IsNullOrWhiteSpace(model.Requirements))
                return ServiceResult.Error("Требования к кандидату обязательны");

            // Валидация зарплаты
            if (model.SalaryFrom.HasValue && model.SalaryTo.HasValue)
            {
                if (model.SalaryFrom.Value > model.SalaryTo.Value)
                    return ServiceResult.Error("Зарплата 'от' не может быть больше зарплаты 'до'");

                if (model.SalaryFrom.Value < 0 || model.SalaryTo.Value < 0)
                    return ServiceResult.Error("Зарплата не может быть отрицательной");
            }

            // Валидация типов
            var validEmploymentTypes = new[] { "full", "part", "project", "internship", "volunteer" };
            if (!validEmploymentTypes.Contains(model.EmploymentType))
                return ServiceResult.Error("Недопустимый тип занятости");

            var validWorkSchedules = new[] { "full_day", "shifts", "flexible", "remote", "shift_work" };
            if (!validWorkSchedules.Contains(model.WorkSchedule))
                return ServiceResult.Error("Недопустимый график работы");

            return ServiceResult.Success("Валидация пройдена");
        }

        public ServiceResult<Vacancy> UpdateVacancy(string companyName, string title, CreateVacancyViewModel model)
        {
            throw new NotImplementedException();
        }

        public Vacancy? GetVacancy(string companyName, string title)
        {
            return _vacancyRepository.GetByCompanyAndTitle(companyName, title);
        }

        public ICollection<Vacancy> GetUserVacancies(string authorEmail)
        {
            return _vacancyRepository.GetByAuthor(authorEmail);
        }

        public ICollection<Vacancy> GetAllVacancies()
        {
            return _vacancyRepository.GetPublishedVacancies();
        }
    }
}