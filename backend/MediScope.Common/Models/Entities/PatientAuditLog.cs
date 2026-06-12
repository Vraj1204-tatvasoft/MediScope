namespace MediScope.Common.Models.Entities
{
    public class PatientAuditLog : BaseEntity
    {
        public Guid PatientId { get; set; }

        /// <summary>FK → users.id — who made the change (patient or doctor)</summary>
        public Guid ChangedByUserId { get; set; }

        /// <summary>Name of the column that changed e.g. "address", "blood_group"</summary>
        public string FieldName { get; set; } = null!;

        /// <summary>Value before the change</summary>
        public string? OldValue { get; set; }

        /// <summary>Value after the change</summary>
        public string? NewValue { get; set; }

        /// <summary>When the change happened</summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Patient Patient { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}