using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin;

[Authorize(Roles = "admin")]
public class VacancyEditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public VacancyEditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CreateVacancyViewModel VacancyData { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string companyName, string title)
    {
        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);
        if (vacancy == null)
        {
            return NotFound();
        }

        VacancyData = new CreateVacancyViewModel
        {
            CompanyName = vacancy.CompanyName,
            Region = vacancy.Region,
            Title = vacancy.Title,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            SalaryFrom = vacancy.SalaryFrom,
            SalaryTo = vacancy.SalaryTo,
            EmploymentType = vacancy.EmploymentType,
            WorkSchedule = vacancy.WorkSchedule,
            WorkHoursPerDay = vacancy.WorkHoursPerDay,
            WorkFormat = vacancy.WorkFormat,
            SalaryPeriod = vacancy.SalaryPeriod,
            PaymentFrequency = vacancy.PaymentFrequency,
            Specialty = vacancy.Specialty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string companyName, string title)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);
        if (vacancy == null)
        {
            return NotFound();
        }

        vacancy.CompanyName = VacancyData.CompanyName.Trim();
        vacancy.Region = VacancyData.Region.Trim();
        vacancy.Title = VacancyData.Title.Trim();
        vacancy.Description = VacancyData.Description.Trim();
        vacancy.Requirements = VacancyData.Requirements.Trim();
        vacancy.SalaryFrom = VacancyData.SalaryFrom;
        vacancy.SalaryTo = VacancyData.SalaryTo;
        vacancy.EmploymentType = VacancyData.EmploymentType;
        vacancy.WorkSchedule = VacancyData.WorkSchedule;
        vacancy.WorkHoursPerDay = VacancyData.WorkHoursPerDay;
        vacancy.WorkFormat = VacancyData.WorkFormat?.Trim() ?? string.Empty;
        vacancy.SalaryPeriod = VacancyData.SalaryPeriod?.Trim() ?? string.Empty;
        vacancy.PaymentFrequency = VacancyData.PaymentFrequency?.Trim() ?? string.Empty;
        vacancy.Specialty = VacancyData.Specialty?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Вакансия обновлена администратором.";
        return RedirectToPage("/Admin/Vacancies/Vacancies");
    }
}
