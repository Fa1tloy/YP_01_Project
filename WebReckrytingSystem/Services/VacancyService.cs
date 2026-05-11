// Services/VacancyService.cs
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
            if (user == null || user.Role != User.ROLE_ADMIN)
            {
                _logger.LogWarning($"Попытка создания вакансии не-администратором: {authorEmail}");
                return ServiceResult<Vacancy>.Error("Только администратор может создавать вакансии");
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
                    Region = model.Region.Trim(),
                    Title = model.Title.Trim(),
                    Description = model.Description.Trim(),
                    Requirements = model.Requirements.Trim(),
                    SalaryFrom = model.SalaryFrom,
                    EmploymentType = model.EmploymentType,
                    WorkSchedule = model.WorkSchedule,
                    WorkHoursPerDay = model.WorkHoursPerDay,
                    WorkFormat = model.WorkFormat.Trim(),
                    Specialty = model.Specialty.Trim(),
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
            if (model.SalaryFrom.HasValue && model.SalaryFrom.Value < 0)
                return ServiceResult.Error("Зарплата не может быть отрицательной");

            // Валидация типов
            var validEmploymentTypes = new[] { "full", "part", "project", "internship" };
            if (!validEmploymentTypes.Contains(model.EmploymentType))
                return ServiceResult.Error("Недопустимый тип занятости");

            var validWorkSchedules = new[] { "full_day", "shifts", "flexible", "remote", "shift_work" };
            var validWorkFormats = new[] { "office", "remote", "travel", "hybrid" };
            if (!validWorkSchedules.Contains(model.WorkSchedule))
                return ServiceResult.Error("Недопустимый график работы");

            if (!string.IsNullOrWhiteSpace(model.WorkFormat) && !validWorkFormats.Contains(model.WorkFormat))
                return ServiceResult.Error("Недопустимый формат работы");

            return ServiceResult.Success("Валидация пройдена");
        }

        public ServiceResult<Vacancy> UpdateVacancy(string companyName, string title, string userEmail, CreateVacancyViewModel model)
        {
            _logger.LogInformation($"Обновление вакансии: {companyName} - {title} пользователем: {userEmail}");
            _logger.LogInformation($"Новые данные: Company={model.CompanyName}, Title={model.Title}");

            // 1. Проверка прав доступа
            var user = _userRepository.FindByEmail(userEmail);
            if (user == null || user.Role != User.ROLE_ADMIN)
            {
                _logger.LogWarning($"Попытка редактирования вакансии не-администратором: {userEmail}");
                return ServiceResult<Vacancy>.Error("Только администратор может редактировать вакансии");
            }

            // 2. Поиск вакансии
            var existingVacancy = _vacancyRepository.GetByCompanyAndTitle(companyName, title);
            if (existingVacancy == null)
            {
                _logger.LogWarning($"Вакансия не найдена: {companyName} - {title}");
                return ServiceResult<Vacancy>.Error("Вакансия не найдена");
            }

            // 3. Проверка компании
            var company = _companyRepository.FindByName(model.CompanyName.Trim());
            if (company == null)
            {
                _logger.LogWarning($"Компания не найдена: {model.CompanyName}");
                return ServiceResult<Vacancy>.Error("Компания не найдена");
            }

            // 4. Проверка дубликатов (если изменилось название)
            if (!string.Equals(existingVacancy.Title, model.Title.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var duplicateVacancy = _vacancyRepository.GetByCompanyAndTitle(model.CompanyName, model.Title);
                if (duplicateVacancy != null)
                {
                    _logger.LogWarning($"Дубликат вакансии: {model.CompanyName} - {model.Title}");
                    return ServiceResult<Vacancy>.Error("Вакансия с таким названием уже существует в этой компании");
                }
            }

            // 5. Валидация данных вакансии
            var validationResult = ValidateVacancyData(model);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning($"Ошибка валидации: {validationResult.Message}");
                return ServiceResult<Vacancy>.Error(validationResult.Message);
            }

            try
            {
                // 6. Обновление вакансии
                existingVacancy.CompanyName = model.CompanyName.Trim();
                existingVacancy.Region = model.Region.Trim();
                existingVacancy.Title = model.Title.Trim();
                existingVacancy.Description = model.Description.Trim();
                existingVacancy.Requirements = model.Requirements.Trim();
                existingVacancy.SalaryFrom = model.SalaryFrom;
                existingVacancy.EmploymentType = model.EmploymentType;
                existingVacancy.WorkSchedule = model.WorkSchedule;
                existingVacancy.WorkHoursPerDay = model.WorkHoursPerDay;
                existingVacancy.WorkFormat = model.WorkFormat.Trim();
                existingVacancy.Specialty = model.Specialty.Trim();

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