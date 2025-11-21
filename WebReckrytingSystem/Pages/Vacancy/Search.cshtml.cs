using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            _vacancySearchService = vacancySearchService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public SearchVacancyViewModel SearchData { get; set; } = new();

        public SearchResult<WebReckrytingSystem.Models.Vacancy> SearchResult { get; set; } = new();

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("GET запрос на страницу поиска вакансий");

            // Проверяем, есть ли параметры поиска
            var hasSearchParameters = !string.IsNullOrEmpty(SearchData.Keywords) ||
                                    !string.IsNullOrEmpty(SearchData.CompanyName) ||
                                    SearchData.SalaryFrom.HasValue ||
                                    SearchData.SalaryTo.HasValue ||
                                    !string.IsNullOrEmpty(SearchData.EmploymentType) ||
                                    !string.IsNullOrEmpty(SearchData.WorkSchedule);

            if (hasSearchParameters)
            {
                _logger.LogInformation("Выполнение поиска с параметрами");
                var result = _vacancySearchService.SearchVacancies(SearchData);

                if (result.IsSuccess)
                {
                    SearchResult = result.Data!;
                    SuccessMessage = result.Message;
                    _logger.LogInformation($"Поиск успешен: {SuccessMessage}");
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning($"Ошибка поиска: {ErrorMessage}");
                }
            }
            else
            {
                _logger.LogInformation("Параметры поиска отсутствуют - показ пустой формы");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("POST запрос на поиск вакансий");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Модель поиска не валидна");
                return Page();
            }

            // Редирект на GET с параметрами в query string
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(SearchData.Keywords))
                queryParams.Add($"Keywords={Uri.EscapeDataString(SearchData.Keywords)}");

            if (!string.IsNullOrEmpty(SearchData.CompanyName))
                queryParams.Add($"CompanyName={Uri.EscapeDataString(SearchData.CompanyName)}");

            if (SearchData.SalaryFrom.HasValue)
                queryParams.Add($"SalaryFrom={SearchData.SalaryFrom}");

            if (SearchData.SalaryTo.HasValue)
                queryParams.Add($"SalaryTo={SearchData.SalaryTo}");

            if (!string.IsNullOrEmpty(SearchData.EmploymentType))
                queryParams.Add($"EmploymentType={SearchData.EmploymentType}");

            if (!string.IsNullOrEmpty(SearchData.WorkSchedule))
                queryParams.Add($"WorkSchedule={SearchData.WorkSchedule}");

            queryParams.Add($"Page={SearchData.Page}");
            queryParams.Add($"PageSize={SearchData.PageSize}");

            var queryString = string.Join("&", queryParams);
            var redirectUrl = string.IsNullOrEmpty(queryString) ? "/Vacancy/Search" : $"/Vacancy/Search?{queryString}";

            _logger.LogInformation($"Редирект на: {redirectUrl}");
            return Redirect(redirectUrl);
        }
    }
}