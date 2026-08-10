using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Broadcast : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public BroadcastChannel Channel { get; set; }

        public string? Subject { get; set; }

        public string Message { get; set; } = string.Empty;

        public BroadcastAudience Audience { get; set; }

        public BroadcastStatus Status { get; set; } = BroadcastStatus.Draft;

        public int TotalRecipients { get; set; }

        public int SentCount { get; set; }

        public int FailedCount { get; set; }

        public string? HangfireJobId { get; set; }

        public int BatchSize { get; set; } = 100;

        public DateTime? ScheduledAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? FailureReason { get; set; }
        public int? RemainingBatches { get; set; }

        public ICollection<BroadcastRecipient> Recipients { get; set; }
            = new List<BroadcastRecipient>();
    }
}