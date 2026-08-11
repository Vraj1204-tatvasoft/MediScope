namespace MediScope.Common.Models.Entities
{
    public class PatientAuditLog : BaseEntity
    {
        public Guid PatientId { get; set; }

        public Guid ChangedByUserId { get; set; }

        public string FieldName { get; set; } = null!;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public Patient Patient { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}