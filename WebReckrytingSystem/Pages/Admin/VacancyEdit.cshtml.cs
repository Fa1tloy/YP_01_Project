using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Admin;

[Authorize(Roles = "admin")]
public class VacancyEditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ISpecialtyService _specialtyService;

    public VacancyEditModel(ApplicationDbContext context, ISpecialtyService specialtyService)
    {
        _context = context;
        _specialtyService = specialtyService;
    }

    [BindProperty]
    public CreateVacancyViewModel VacancyData { get; set; } = new();

    public List<SelectListItem> CompanyOptions { get; set; } = new();
    public IReadOnlyList<string> Specialties { get; set; } = new List<string>();

    public async Task<IActionResult> OnGetAsync(string companyName, string title)
    {
        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);
        if (vacancy == null)
            return NotFound();

        VacancyData = new CreateVacancyViewModel
        {
            CompanyName = vacancy.CompanyName,
            Region = vacancy.Region,
            Title = vacancy.Title,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            SalaryFrom = vacancy.SalaryFrom,
            EmploymentType = vacancy.EmploymentType,
            WorkSchedule = vacancy.WorkSchedule,
            WorkHoursPerDay = vacancy.WorkHoursPerDay,
            WorkFormat = vacancy.WorkFormat,
            Specialty = vacancy.Specialty
        };

        Specialties = _specialtyService.GetAllNames();
        LoadCompanyOptions();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string companyName, string title)
    {
        Specialties = _specialtyService.GetAllNames();
        LoadCompanyOptions();

        if (!ModelState.IsValid)
            return Page();

        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.CompanyName == companyName && v.Title == title);
        if (vacancy == null)
            return NotFound();

        vacancy.CompanyName = VacancyData.CompanyName.Trim();
        vacancy.Region = VacancyData.Region.Trim();
        vacancy.Title = VacancyData.Title.Trim();
        vacancy.Description = VacancyData.Description.Trim();
        vacancy.Requirements = VacancyData.Requirements.Trim();
        vacancy.SalaryFrom = VacancyData.SalaryFrom;
        vacancy.EmploymentType = VacancyData.EmploymentType;
        vacancy.WorkSchedule = VacancyData.WorkSchedule;
        vacancy.WorkHoursPerDay = VacancyData.WorkHoursPerDay;
        vacancy.WorkFormat = VacancyData.WorkFormat?.Trim() ?? string.Empty;
        vacancy.Specialty = VacancyData.Specialty?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Вакансия обновлена администратором.";
        return RedirectToPage("/Admin/Vacancies/Vacancies");
    }

    private void LoadCompanyOptions()
    {
        CompanyOptions = _context.Companies
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            })
            .ToList();
    }
}