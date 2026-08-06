using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateBroadcastRequestDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public BroadcastChannel Channel { get; set; }

        [MaxLength(500)]
        public string? Subject { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public BroadcastAudience Audience { get; set; }

        public List<Guid>? CustomUserIds { get; set; }

        public bool SendNow { get; set; } = false;

        public int BatchSize { get; set; } = 100;
    }
}