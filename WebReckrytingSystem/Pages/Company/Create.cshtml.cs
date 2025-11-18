using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Company
{
    [Authorize(Roles = "employer")]
    public class CreateModel : PageModel
    {
        private readonly ICompanyService _companyService;

        public CreateModel(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [BindProperty]
        public CreateCompanyViewModel CompanyData { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                ErrorMessage = "Ошибка аутентификации";
                return Page();
            }

            var result = _companyService.CreateCompany(userEmail, CompanyData);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Компания успешно создана! Теперь вы можете создать вакансию.";
                return RedirectToPage("/Vacancy/Create");
            }
            else
            {
                ErrorMessage = result.Message;
                return Page();
            }
        }
    }
}