using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;

namespace WebReckrytingSystem.Pages.Admin.Specialties
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly ISpecialtyService _specialtyService;

        public IndexModel(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        public IReadOnlyList<Specialty> Items { get; set; } = new List<Specialty>();

        [BindProperty]
        public string NewSpecialty { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            Items = _specialtyService.GetAll();
        }

        public IActionResult OnPostAdd()
        {
            if (string.IsNullOrWhiteSpace(NewSpecialty))
            {
                ErrorMessage = "Введите название специальности.";
                Items = _specialtyService.GetAll();
                return Page();
            }

            try
            {
                _specialtyService.Add(NewSpecialty.Trim());
                SuccessMessage = $"Специальность \"{NewSpecialty.Trim()}\" добавлена.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            Items = _specialtyService.GetAll();
            return Page();
        }

        public IActionResult OnPostDelete(int id)
        {
            try
            {
                _specialtyService.Delete(id);
                SuccessMessage = "Специальность удалена.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при удалении: {ex.Message}";
            }

            Items = _specialtyService.GetAll();
            return Page();
        }
    }
}