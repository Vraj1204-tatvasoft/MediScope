using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class ValidateResetTokenRequestDto
    {
        [Required]
        public string Token { get; set; } = null!;
    }
}