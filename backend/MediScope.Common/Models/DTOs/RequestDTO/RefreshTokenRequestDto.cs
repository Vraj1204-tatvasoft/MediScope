using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Auth
{

    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}