using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebReckrytingSystem.Models
{
    public class PracticeViewModel
    {
        [Required(ErrorMessage = "Укажите место практики")]
        [StringLength(255, ErrorMessage = "Место практики не должно превышать 255 символов")]
        public string Place { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите дату начала")]
        [DataType(DataType.Date)]
        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Укажите дату окончания")]
        [DataType(DataType.Date)]
        [Column("EndDate")]
        public DateTime EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        // Валидация: дата окончания >= дата начала
        public bool IsValid => EndDate >= StartDate;
    }
}