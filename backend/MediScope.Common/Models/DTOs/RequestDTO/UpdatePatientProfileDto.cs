
using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Patient
{
    public class UpdatePatientProfileDto
    {
        public DateOnly? DateOfBirth { get; set; }

        public Enums.Gender? Gender { get; set; }

        [MaxLength(5)]
        public string? BloodGroup { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public string? ContactNumber { get; set; }

        public string? Address { get; set; }

        public bool? ConsentProfileVisible { get; set; }
    }
}