using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least 8 characters, including uppercase, lowercase, and number."
        )]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}