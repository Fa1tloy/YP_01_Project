using Microsoft.Extensions.Logging;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Services
{
    public class VacancySearchService : IVacancySearchService
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<VacancySearchService> _logger;

        public VacancySearchService(IVacancyRepository vacancyRepository, ILogger<VacancySearchService> logger)
        {
            _vacancyRepository = vacancyRepository;
            _logger = logger;
        }

        public ServiceResult<SearchResult<Vacancy>> SearchVacancies(SearchVacancyViewModel model)
        {
            try
            {
                _logger.LogInformation($"Начало поиска вакансий. Ключевые слова: {model.Keywords}");

                // 1. Валидация входных данных
                var validationResult = ValidateSearchModel(model);
                if (!validationResult.IsSuccess)
                {
                    _logger.LogWarning($"Ошибка валидации: {validationResult.Message}");
                    return ServiceResult<SearchResult<Vacancy>>.Error(validationResult.Message);
                }

                // 2. Получение всех вакансий
                var allVacancies = _vacancyRepository.GetPublishedVacancies();
                _logger.LogInformation($"Получено {allVacancies.Count} вакансий для поиска");

                // 3. Применение фильтров
                var filteredVacancies = ApplyFilters(allVacancies, model);
                _logger.LogInformation($"После фильтрации осталось {filteredVacancies.Count} вакансий");

                // 4. Сортировка
                var sortedVacancies = SortVacancies(filteredVacancies, model.Keywords);

                // 5. Применение пагинации
                var result = ApplyPagination(sortedVacancies, model);

                var message = result.TotalCount == 0
                    ? "По вашему запросу ничего не найдено"
                    : $"Найдено {result.TotalCount} вакансий";

                _logger.LogInformation($"Поиск завершен: {message}");
                return ServiceResult<SearchResult<Vacancy>>.Success(message, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске вакансий");
                return ServiceResult<SearchResult<Vacancy>>.Error("Произошла ошибка при поиске вакансий");
            }
        }

        private ServiceResult ValidateSearchModel(SearchVacancyViewModel model)
        {
            // Валидация зарплаты
            if (model.SalaryFrom.HasValue && model.SalaryFrom < 0)
                return ServiceResult.Error("Зарплата не может быть отрицательной");

            if (model.SalaryTo.HasValue && model.SalaryTo < 0)
                return ServiceResult.Error("Зарплата не может быть отрицательной");

            if (model.SalaryFrom.HasValue && model.SalaryTo.HasValue && model.SalaryFrom > model.SalaryTo)
                return ServiceResult.Error("Зарплата 'от' не может быть больше зарплаты 'до'");

            // Валидация типов
            if (!string.IsNullOrEmpty(model.EmploymentType))
            {
                var validEmploymentTypes = new[] { "full", "part", "project", "internship", "volunteer" };
                if (!validEmploymentTypes.Contains(model.EmploymentType))
                    return ServiceResult.Error("Недопустимый тип занятости");
            }

            if (!string.IsNullOrEmpty(model.WorkSchedule))
            {
                var validWorkSchedules = new[] { "full_day", "shifts", "flexible", "remote", "shift_work" };
                if (!validWorkSchedules.Contains(model.WorkSchedule))
                    return ServiceResult.Error("Недопустимый график работы");
            }

            if (model.WorkHoursPerDay.HasValue && (model.WorkHoursPerDay < 1 || model.WorkHoursPerDay > 24))
                return ServiceResult.Error("Рабочие часы в день должны быть от 1 до 24");

            // Валидация пагинации
            if (model.Page < 1 || model.Page > 100)
                return ServiceResult.Error("Некорректный номер страницы");

            if (model.PageSize < 1 || model.PageSize > 50)
                return ServiceResult.Error("Некорректный размер страницы");

            return ServiceResult.Success("Валидация пройдена");
        }

        private ICollection<Vacancy> ApplyFilters(ICollection<Vacancy> vacancies, SearchVacancyViewModel model)
        {
            var filtered = vacancies.AsEnumerable();

            // Фильтр по ключевым словам
            if (!string.IsNullOrWhiteSpace(model.Keywords))
            {
                var keywords = model.Keywords.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                filtered = filtered.Where(v =>
                    keywords.Any(k =>
                        (v.Title?.ToLower().Contains(k) == true) ||
                        (v.Description?.ToLower().Contains(k) == true) ||
                        (v.Requirements?.ToLower().Contains(k) == true) ||
                        (v.CompanyName?.ToLower().Contains(k) == true)
                    )
                );
            }

            // Фильтр по компании
            if (!string.IsNullOrWhiteSpace(model.CompanyName))
            {
                filtered = filtered.Where(v =>
                    v.CompanyName.Contains(model.CompanyName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(model.Region))
            {
                filtered = filtered.Where(v =>
                    !string.IsNullOrWhiteSpace(v.Region) &&
                    v.Region.Contains(model.Region, StringComparison.OrdinalIgnoreCase));
            }

            // Фильтр по зарплате
            if (model.SalaryFrom.HasValue)
            {
                filtered = filtered.Where(v =>
                    v.SalaryTo >= model.SalaryFrom || v.SalaryFrom >= model.SalaryFrom);
            }

            if (model.SalaryTo.HasValue)
            {
                filtered = filtered.Where(v =>
                    v.SalaryFrom <= model.SalaryTo || (v.SalaryTo.HasValue && v.SalaryTo <= model.SalaryTo));
            }

            // Фильтр по типу занятости
            if (!string.IsNullOrEmpty(model.EmploymentType))
            {
                filtered = filtered.Where(v => v.EmploymentType == model.EmploymentType);
            }

            // Фильтр по графику работы
            if (!string.IsNullOrEmpty(model.WorkSchedule))
            {
                filtered = filtered.Where(v => v.WorkSchedule == model.WorkSchedule);
            }

            if (model.WorkHoursPerDay.HasValue)
            {
                filtered = filtered.Where(v => v.WorkHoursPerDay == model.WorkHoursPerDay);
            }

            if (!string.IsNullOrWhiteSpace(model.WorkFormat))
            {
                filtered = filtered.Where(v => v.WorkFormat == model.WorkFormat);
            }

            if (!string.IsNullOrWhiteSpace(model.SalaryPeriod))
            {
                filtered = filtered.Where(v => v.SalaryPeriod == model.SalaryPeriod);
            }

            if (!string.IsNullOrWhiteSpace(model.PaymentFrequency))
            {
                filtered = filtered.Where(v => v.PaymentFrequency == model.PaymentFrequency);
            }

            if (!string.IsNullOrWhiteSpace(model.Specialty))
            {
                filtered = filtered.Where(v => v.Specialty == model.Specialty);
            }

            return filtered.ToList();
        }

        private ICollection<Vacancy> SortVacancies(ICollection<Vacancy> vacancies, string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords))
            {
                // Сортировка по названию компании и должности
                return vacancies.OrderByDescending(v => v.CompanyName).ThenBy(v => v.Title).ToList();
            }

            // Сортировка по релевантности при поиске по ключевым словам
            var keywordsArray = keywords.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return vacancies.OrderByDescending(v => CalculateRelevanceScore(v, keywordsArray))
                           .ThenByDescending(v => v.CompanyName)
                           .ThenBy(v => v.Title)
                           .ToList();
        }

        private int CalculateRelevanceScore(Vacancy vacancy, string[] keywords)
        {
            int score = 0;
            var title = vacancy.Title?.ToLower() ?? "";
            var description = vacancy.Description?.ToLower() ?? "";
            var requirements = vacancy.Requirements?.ToLower() ?? "";
            var company = vacancy.CompanyName?.ToLower() ?? "";

            foreach (var keyword in keywords)
            {
                if (title.Contains(keyword)) score += 10;
                if (requirements.Contains(keyword)) score += 5;
                if (description.Contains(keyword)) score += 3;
                if (company.Contains(keyword)) score += 2;
            }

            return score;
        }

        private SearchResult<Vacancy> ApplyPagination(ICollection<Vacancy> vacancies, SearchVacancyViewModel model)
        {
            var page = model.Page;
            var pageSize = model.PageSize;
            var totalCount = vacancies.Count;

            var items = vacancies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new SearchResult<Vacancy>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public ServiceResult<SearchResult<Vacancy>> GetSimilarVacancies(Vacancy vacancy, int count = 5)
        {
            // Базовая реализация поиска похожих вакансий
            try
            {
                var allVacancies = _vacancyRepository.GetPublishedVacancies();

                // Исключаем текущую вакансию
                var similarVacancies = allVacancies
                    .Where(v => v.CompanyName != vacancy.CompanyName || v.Title != vacancy.Title)
                    .Take(count)
                    .ToList();

                var result = new SearchResult<Vacancy>
                {
                    Items = similarVacancies,
                    TotalCount = similarVacancies.Count,
                    Page = 1,
                    PageSize = count
                };

                return ServiceResult<SearchResult<Vacancy>>.Success("Похожие вакансии", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске похожих вакансий");
                return ServiceResult<SearchResult<Vacancy>>.Error("Ошибка при поиске похожих вакансий");
            }
        }
    }
}
