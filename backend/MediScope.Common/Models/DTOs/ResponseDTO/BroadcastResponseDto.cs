using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Response
{
    public class BroadcastResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BroadcastChannel Channel { get; set; }
        public string ChannelDisplay => Channel.ToString();
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
        public BroadcastAudience Audience { get; set; }
        public string AudienceDisplay => Audience.ToString();
        public BroadcastStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public string? HangfireJobId { get; set; }
        public int BatchSize { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? FailureReason { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal ProgressPercent =>
            TotalRecipients > 0
                ? Math.Round(((decimal)(SentCount + FailedCount) / TotalRecipients) * 100, 2)
                : 0;
    }
}