namespace MediScope.Common.Models.Entities
{
    /// <summary>
    /// Stores all notifications for any user (patient or doctor).
    /// Type determines the icon and colour on the frontend.
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>The user who receives this notification</summary>
        public Guid UserId { get; set; }

        /// <summary>alert | info | success | reminder</summary>
        public string Type { get; set; } = "info";

        /// <summary>The notification message text</summary>
        public string Message { get; set; } = null!;

        /// <summary>Whether the user has read it</summary>
        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}