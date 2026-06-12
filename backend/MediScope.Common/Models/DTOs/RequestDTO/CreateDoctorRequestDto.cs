using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateDoctorRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LicenseNumber { get; set; } = null!;

        [Required]
        [RegularExpression(@"^\d{10}$")]
        public string? ContactNumber { get; set; }

        [MaxLength(150)]
        public string? Hospital { get; set; }

        [Range(0, 60, ErrorMessage = "Years of experience must be between 0 and 60.")]
        public int? YearsExperience { get; set; }
        public string? Bio { get; set; }
    }
}