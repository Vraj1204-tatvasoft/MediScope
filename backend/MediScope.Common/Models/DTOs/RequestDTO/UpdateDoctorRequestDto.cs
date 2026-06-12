using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateDoctorRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = null!;

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public string? ContactNumber { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(150)]
        public string? Hospital { get; set; }

        [Range(0, 60, ErrorMessage = "Years of experience must be between 0 and 60.")]
        public int? YearsExperience { get; set; }

        public string? Bio { get; set; }
    }
}