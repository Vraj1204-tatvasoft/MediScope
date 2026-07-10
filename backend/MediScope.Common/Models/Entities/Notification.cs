using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Notification : BaseEntity
    {
        /// <summary>The user who receives this notification</summary>
        public Guid UserId { get; set; }

        /// <summary>alert | info | success | reminder</summary>
        public NotificationType Type { get; set; } = NotificationType.Info;

        /// <summary>The notification message text</summary>
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public User User { get; set; } = null!;
    }
}