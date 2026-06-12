namespace MediScope.Common.Models.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid PerformedBy { get; set; }
        public string Action { get; set; } = null!;       // INSERT | UPDATE | DELETE | CONSENT_TOGGLE
        public string EntityType { get; set; } = null!;
        public Guid EntityId { get; set; }
        public string? Changes { get; set; }              // JSON before/after diff
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User PerformedByUser { get; set; } = null!;
    }
}