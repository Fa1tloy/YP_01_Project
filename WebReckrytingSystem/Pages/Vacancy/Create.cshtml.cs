using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Vacancy
{
    [Authorize(Roles = "employer")]
    public class CreateModel : PageModel
    {
        private readonly IVacancyService _vacancyService;
        private readonly ICompanyRepository _companyRepository;

        public CreateModel(IVacancyService vacancyService, ICompanyRepository companyRepository)
        {
            _vacancyService = vacancyService;
            _companyRepository = companyRepository;
        }

        [BindProperty]
        public CreateVacancyViewModel VacancyData { get; set; } = new();

        public List<Company> Companies { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmail))
            {
                Companies = _companyRepository.GetUserCompanies(userEmail).ToList();
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    Companies = _companyRepository.GetUserCompanies(userEmail).ToList();
                }
                return Page();
            }

            var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmailClaim))
            {
                ErrorMessage = "Ошибка аутентификации";
                return Page();
            }

            var result = _vacancyService.CreateVacancy(userEmailClaim, VacancyData);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Вакансия успешно опубликована!";
                return RedirectToPage("/Account/EmployerDashboard");
            }
            else
            {
                ErrorMessage = result.Message;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    Companies = _companyRepository.GetUserCompanies(userEmail).ToList();
                }
                return Page();
            }
        }
    }
}