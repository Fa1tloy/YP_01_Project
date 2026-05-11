using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Vacancy
{
    public class SearchModel : PageModel
    {
        private readonly IVacancySearchService _vacancySearchService;
        private readonly ISpecialtyService _specialtyService;
        private readonly ILogger<SearchModel> _logger;

        public SearchModel(IVacancySearchService vacancySearchService,
                           ISpecialtyService specialtyService,
                           ILogger<SearchModel> logger)
        {
            _vacancySearchService = vacancySearchService ?? throw new ArgumentNullException(nameof(vacancySearchService));
            _specialtyService = specialtyService ?? throw new ArgumentNullException(nameof(specialtyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty(SupportsGet = true)]
        public SearchVacancyViewModel SearchData { get; set; } = new() { Page = 1, PageSize = 10 };

        public SearchResult<WebReckrytingSystem.Models.Vacancy> SearchResult { get; set; } = new();
        public IReadOnlyList<string> Specialties { get; set; } = new List<string>();
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            Specialties = _specialtyService.GetAllNames();

            try
            {
                _logger.LogInformation("🎯 Поиск вакансий: Запрос с параметрами {@SearchData}", SearchData);

                var result = _vacancySearchService.SearchVacancies(SearchData);

                if (result.IsSuccess && result.Data != null)
                {
                    SearchResult = result.Data;
                    SuccessMessage = result.Message;
                    _logger.LogInformation("✅ Найдено {TotalCount} вакансий (Стр. {Page}/{TotalPages})",
                        SearchResult.TotalCount, SearchResult.Page, SearchResult.TotalPages);
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning("⚠️ Поиск не дал результатов: {Message}", ErrorMessage);
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
            Specialties = _specialtyService.GetAllNames();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("⚠️ Форма поиска не валидна");
                ErrorMessage = "Проверьте введенные данные";
                return Page();
            }

            try
            {
                _logger.LogInformation("📝 Пользователь ищет: {@SearchData}", SearchData);

                var query = new QueryBuilder();
                AddIfNotEmpty(query, "Keywords", SearchData.Keywords);
                AddIfNotEmpty(query, "CompanyName", SearchData.CompanyName);
                AddIfNotEmpty(query, "Region", SearchData.Region);
                AddIfNotNull(query, "SalaryFrom", SearchData.SalaryFrom);
                AddIfNotEmpty(query, "EmploymentType", SearchData.EmploymentType);
                AddIfNotEmpty(query, "WorkSchedule", SearchData.WorkSchedule);
                AddIfNotNull(query, "WorkHoursPerDay", SearchData.WorkHoursPerDay);
                AddIfNotEmpty(query, "WorkFormat", SearchData.WorkFormat);
                AddIfNotEmpty(query, "Specialty", SearchData.Specialty);
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
                Page = SearchData.Page,
                PageSize = SearchData.PageSize
            };
        }
    }
}