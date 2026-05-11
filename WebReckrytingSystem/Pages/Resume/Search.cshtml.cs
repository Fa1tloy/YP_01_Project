// Pages/Resume/Search.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Resume
{
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpecialtyService _specialtyService;

        public SearchModel(ApplicationDbContext context, ISpecialtyService specialtyService)
        {
            _context = context;
            _specialtyService = specialtyService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? City { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? BusinessTripReadiness { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? AgeFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? AgeTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EmploymentType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? WorkSchedule { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Specialty { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Gender { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SalaryExpectationsFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SalaryExpectationsTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Skills { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? HasCar { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> DriverLicenseCategories { get; set; } = new();

        public IReadOnlyList<string> Specialties => _specialtyService.GetAllNames();
        public IReadOnlyList<string> AvailableDriverLicenseCategories => DriverLicenseCategoryCatalog.All;

        public List<Models.Resume> Resumes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _context.Resumes
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.IsPublished);

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var term = SearchQuery.Trim();
                query = query.Where(r =>
                    r.DesiredPosition.Contains(term) ||
                    (r.Skills != null && r.Skills.Contains(term)) ||
                    r.UserEmail.Contains(term) ||
                    ((r.User.FirstName + " " + r.User.LastName).Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(Specialty))
            {
                query = query.Where(r => r.Specialty == Specialty);
            }

            if (!string.IsNullOrWhiteSpace(City))
            {
                query = query.Where(r => r.City.Contains(City));
            }

            if (!string.IsNullOrWhiteSpace(BusinessTripReadiness))
            {
                query = query.Where(r => r.BusinessTripReadiness == BusinessTripReadiness);
            }

            if (!string.IsNullOrWhiteSpace(SearchStatus))
            {
                query = query.Where(r => r.SearchStatus == SearchStatus);
            }

            if (AgeFrom.HasValue)
            {
                query = query.Where(r => r.Age.HasValue && r.Age >= AgeFrom);
            }

            if (AgeTo.HasValue)
            {
                query = query.Where(r => r.Age.HasValue && r.Age <= AgeTo);
            }

            if (!string.IsNullOrWhiteSpace(EmploymentType))
            {
                query = query.Where(r => r.EmploymentType == EmploymentType);
            }

            if (!string.IsNullOrWhiteSpace(WorkSchedule))
            {
                query = query.Where(r => r.WorkSchedule == WorkSchedule);
            }

            if (!string.IsNullOrWhiteSpace(Skills))
            {
                query = query.Where(r => r.Skills != null && r.Skills.Contains(Skills));
            }

            if (!string.IsNullOrWhiteSpace(Gender))
            {
                query = query.Where(r => r.Gender == Gender);
            }

            if (SalaryExpectationsFrom.HasValue)
            {
                query = query.Where(r => r.SalaryExpectations.HasValue && r.SalaryExpectations >= SalaryExpectationsFrom);
            }

            if (SalaryExpectationsTo.HasValue)
            {
                query = query.Where(r => r.SalaryExpectations.HasValue && r.SalaryExpectations <= SalaryExpectationsTo);
            }

            if (!string.IsNullOrWhiteSpace(HasCar))
            {
                var hasCar = HasCar == "yes";
                query = query.Where(r => r.HasCar == hasCar);
            }

            var resumes = await query
                .OrderBy(r => r.DesiredPosition)
                .ToListAsync();

            if (DriverLicenseCategories.Any())
            {
                var selectedCategories = DriverLicenseCategories
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();

                resumes = resumes
                    .Where(r =>
                    {
                        if (string.IsNullOrWhiteSpace(r.DriverLicenseCategory))
                        {
                            return false;
                        }

                        var resumeCategories = r.DriverLicenseCategory
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(c => c.ToUpperInvariant())
                            .ToHashSet();

                        return selectedCategories.All(c => resumeCategories.Contains(c));
                    })
                    .ToList();
            }

            Resumes = resumes.Take(50).ToList();
        }
    }
}