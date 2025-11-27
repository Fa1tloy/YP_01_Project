using Microsoft.Extensions.Logging;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class VacancyService : IVacancyService
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyService _companyService;
        private readonly ILogger<VacancyService> _logger;

        public VacancyService(IVacancyRepository vacancyRepository,
                            ICompanyRepository companyRepository,
                            IUserRepository userRepository,
                            ICompanyService companyService,
                            ILogger<VacancyService> logger)
        {
            _vacancyRepository = vacancyRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _companyService = companyService;
            _logger = logger;
      
        }

        public ServiceResult<Vacancy> CreateVacancy(string authorEmail, CreateVacancyViewModel model)
        {
            _logger.LogInformation($"Создание вакансии для пользователя: {authorEmail}");
            _logger.LogInformation($"Данные вакансии: Company={model.CompanyName}, Title={model.Title}");

            // 1. Проверка прав доступа
            var user = _userRepository.FindByEmail(authorEmail);
            if (user == null || user.Role != "employer")
            {
                _logger.LogWarning($"Попытка создания вакансии не-работодателем: {authorEmail}");
                return ServiceResult<Vacancy>.Error("Только работодатели могут создавать вакансии");
            }

            // 2. Валидация данных вакансии
            var validationResult = ValidateVacancyData(model);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning($"Ошибка валидации: {validationResult.Message}");
                return ServiceResult<Vacancy>.Error(validationResult.Message);
            }

            // 3. Проверка компании
            var company = _companyRepository.FindByName(model.CompanyName.Trim());
            if (company == null)
            {
                _logger.LogWarning($"Компания не найдена: {model.CompanyName}");
                return ServiceResult<Vacancy>.Error("Компания не найдена. Сначала создайте компанию.");
            }

            _logger.LogInformation($"Найдена компания: {company.Name}");

            // 4. Проверка дубликатов
            var existingVacancy = _vacancyRepository.GetByCompanyAndTitle(model.CompanyName, model.Title);
            if (existingVacancy != null)
            {
                _logger.LogWarning($"Дубликат вакансии: {model.CompanyName} - {model.Title}");
                return ServiceResult<Vacancy>.Error("Вакансия с таким названием уже существует в этой компании");
            }

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

                _logger.LogInformation("Сохранение вакансии в репозиторий");
                var savedVacancy = _vacancyRepository.Save(vacancy);
                _logger.LogInformation("Вакансия успешно сохранена");

                return ServiceResult<Vacancy>.Success("Вакансия успешно опубликована!", savedVacancy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении вакансии");
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

        // VacancyService.cs - добавляю метод UpdateVacancy
        public ServiceResult<Vacancy> UpdateVacancy(string companyName, string title, string userEmail, CreateVacancyViewModel model)
        {
            _logger.LogInformation($"Обновление вакансии: {companyName} - {title} пользователем: {userEmail}");
            _logger.LogInformation($"Новые данные: Company={model.CompanyName}, Title={model.Title}");

            // 1. Проверка прав доступа
            var user = _userRepository.FindByEmail(userEmail);
            if (user == null || user.Role != "employer")
            {
                _logger.LogWarning($"Попытка редактирования вакансии не-работодателем: {userEmail}");
                return ServiceResult<Vacancy>.Error("Только работодатели могут редактировать вакансии");
            }

            // 2. Поиск вакансии
            var existingVacancy = _vacancyRepository.GetByCompanyAndTitle(companyName, title);
            if (existingVacancy == null)
            {
                _logger.LogWarning($"Вакансия не найдена: {companyName} - {title}");
                return ServiceResult<Vacancy>.Error("Вакансия не найдена");
            }

            // 3. Проверка авторства
            if (existingVacancy.AuthorEmail != userEmail)
            {
                _logger.LogWarning($"Попытка редактирования чужой вакансии. Автор: {existingVacancy.AuthorEmail}, Пользователь: {userEmail}");
                return ServiceResult<Vacancy>.Error("Вы можете редактировать только свои вакансии");
            }

            // 4. Проверка компании
            var company = _companyRepository.FindByName(model.CompanyName.Trim());
            if (company == null)
            {
                _logger.LogWarning($"Компания не найдена: {model.CompanyName}");
                return ServiceResult<Vacancy>.Error("Компания не найдена");
            }

            // 5. Проверка дубликатов (если изменилось название)
            if (!string.Equals(existingVacancy.Title, model.Title.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var duplicateVacancy = _vacancyRepository.GetByCompanyAndTitle(model.CompanyName, model.Title);
                if (duplicateVacancy != null)
                {
                    _logger.LogWarning($"Дубликат вакансии: {model.CompanyName} - {model.Title}");
                    return ServiceResult<Vacancy>.Error("Вакансия с таким названием уже существует в этой компании");
                }
            }

            // 6. Валидация данных вакансии
            var validationResult = ValidateVacancyData(model);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning($"Ошибка валидации: {validationResult.Message}");
                return ServiceResult<Vacancy>.Error(validationResult.Message);
            }

            try
            {
                // 7. Обновление вакансии
                existingVacancy.CompanyName = model.CompanyName.Trim();
                existingVacancy.Title = model.Title.Trim();
                existingVacancy.Description = model.Description.Trim();
                existingVacancy.Requirements = model.Requirements.Trim();
                existingVacancy.SalaryFrom = model.SalaryFrom;
                existingVacancy.SalaryTo = model.SalaryTo;
                existingVacancy.EmploymentType = model.EmploymentType;
                existingVacancy.WorkSchedule = model.WorkSchedule;

                _logger.LogInformation("Сохранение обновленной вакансии в репозиторий");
                var updatedVacancy = _vacancyRepository.Update(existingVacancy);
                _logger.LogInformation("Вакансия успешно обновлена");

                return ServiceResult<Vacancy>.Success("Вакансия успешно обновлена!", updatedVacancy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении вакансии: {companyName} - {title}");
                return ServiceResult<Vacancy>.Error($"Ошибка при обновлении вакансии: {ex.Message}");
            }
        }

        // Уже существующий метод GetVacancy - убедимся что он есть
        public Vacancy? GetVacancy(string companyName, string title)
        {
            return _vacancyRepository.GetByCompanyAndTitle(companyName, title);
        }

        public ICollection<Vacancy> GetUserVacancies(string authorEmail)
        {
            _logger.LogInformation($"Получение вакансий для пользователя: {authorEmail}");
            var vacancies = _vacancyRepository.GetByAuthor(authorEmail);
            _logger.LogInformation($"Найдено вакансий: {vacancies.Count}");

            foreach (var vacancy in vacancies)
            {
                _logger.LogInformation($"Вакансия: {vacancy.CompanyName} - {vacancy.Title}");
            }

            return vacancies;
        }

        public ICollection<Vacancy> GetAllVacancies()
        {
            return _vacancyRepository.GetPublishedVacancies();
        }
    }
}