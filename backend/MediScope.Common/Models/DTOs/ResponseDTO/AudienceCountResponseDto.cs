using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Response
{
    public class AudienceCountResponseDto
    {
        public BroadcastAudience Audience { get; set; }
        public string AudienceDisplay => Audience.ToString();
        public int TotalRecipients { get; set; }
    }
}