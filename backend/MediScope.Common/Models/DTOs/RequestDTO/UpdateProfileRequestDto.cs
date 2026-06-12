using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateProfileRequestDto
    {
        [Required]
        [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [RegularExpression(
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            ErrorMessage = "Invalid email format."
        )]
        public string Email { get; set; } = null!;

        public DateOnly? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(10, ErrorMessage = "Blood group cannot exceed 10 characters.")]
        public string? BloodGroup { get; set; }

        [RegularExpression(
            @"^\d{10}$",
            ErrorMessage = "Contact number must be exactly 10 digits."
        )]
        public string? ContactNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }
    }
}