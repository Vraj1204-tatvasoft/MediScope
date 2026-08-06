using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class BroadcastRecipient : BaseEntity
    {
        public Guid BroadcastId { get; set; }

        public Broadcast Broadcast { get; set; } = null!;

        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public RecipientStatus Status { get; set; } = RecipientStatus.Pending;

        public DateTime? SentAt { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; }

        public int BatchNumber { get; set; } = 1;
    }
}