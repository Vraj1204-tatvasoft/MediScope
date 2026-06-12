using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public bool MustChangePassword { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
        public UserProfileDto User { get; set; } = null!;
    }
}