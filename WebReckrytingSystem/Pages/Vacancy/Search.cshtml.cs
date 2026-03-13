using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities; // Для QueryBuilder
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Vacancy
{
    public class SearchModel : PageModel
    {
        private readonly IVacancySearchService _vacancySearchService;
        private readonly ILogger<SearchModel> _logger;

        public SearchModel(IVacancySearchService vacancySearchService, ILogger<SearchModel> logger)
        {
            _vacancySearchService = vacancySearchService ?? throw new ArgumentNullException(nameof(vacancySearchService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty(SupportsGet = true)]
        public SearchVacancyViewModel SearchData { get; set; } = new() { Page = 1, PageSize = 10 };

        public SearchResult<WebReckrytingSystem.Models.Vacancy> SearchResult { get; set; } = new();
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("🎯 Поиск вакансий: Запрос с параметрами {@SearchData}", SearchData);

                // Всегда выполняем поиск - даже без параметров показываем все вакансии
                var result = _vacancySearchService.SearchVacancies(SearchData);

                if (result.IsSuccess && result.Data != null)
                {
                    SearchResult = result.Data;
                    SuccessMessage = result.Message;
                    _logger.LogInformation("✅ Найдено {TotalCount} вакансий (Стр. {Page}/{TotalPages})",
                        SearchResult.TotalCount, SearchResult.PageNumber, SearchResult.TotalPages);
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning("⚠️ Поиск не дал результатов: {Message}", ErrorMessage);

                    // Возвращаем пустой результат, чтобы страница не падала
                    SearchResult = CreateEmptyResult();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка поиска вакансий");
                ErrorMessage = "Произошла ошибка при загрузке вакансий";
                SearchResult = CreateEmptyResult();
                return Page();
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("⚠️ Форма поиска не валидна");
                ErrorMessage = "Проверьте введенные данные";
                return Page();
            }

            try
            {
                _logger.LogInformation("📝 Пользователь ищет: {@SearchData}", SearchData);

                // Строим query string
                var query = new QueryBuilder();
                AddIfNotEmpty(query, "Keywords", SearchData.Keywords);
                AddIfNotEmpty(query, "CompanyName", SearchData.CompanyName);
                AddIfNotNull(query, "SalaryFrom", SearchData.SalaryFrom);
                AddIfNotNull(query, "SalaryTo", SearchData.SalaryTo);
                AddIfNotEmpty(query, "EmploymentType", SearchData.EmploymentType);
                AddIfNotEmpty(query, "WorkSchedule", SearchData.WorkSchedule);
                query.Add("Page", SearchData.Page.ToString());
                query.Add("PageSize", SearchData.PageSize.ToString());

                var redirectUrl = $"/Vacancy/Search{query.ToQueryString()}";
                _logger.LogInformation("🔄 Редирект на: {Url}", redirectUrl);

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка обработки формы поиска");
                ErrorMessage = "Не удалось выполнить поиск";
                return Page();
            }
        }

        // Вспомогательные методы
        private void AddIfNotEmpty(QueryBuilder query, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                query.Add(key, value);
        }

        private void AddIfNotNull<T>(QueryBuilder query, string key, T? value) where T : struct
        {
            if (value.HasValue)
                query.Add(key, value.Value.ToString());
        }

        private SearchResult<WebReckrytingSystem.Models.Vacancy> CreateEmptyResult()
        {
            return new SearchResult<WebReckrytingSystem.Models.Vacancy>
            {
                Items = new List<WebReckrytingSystem.Models.Vacancy>(),
                TotalCount = 0,
                PageNumber = SearchData.Page,
                PageSize = SearchData.PageSize
                // TotalPages, HasPreviousPage, HasNextPage вычисляются сами!
            };
        }
    }
}