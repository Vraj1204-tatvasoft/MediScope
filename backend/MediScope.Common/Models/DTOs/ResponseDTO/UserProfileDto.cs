using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Auth
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}