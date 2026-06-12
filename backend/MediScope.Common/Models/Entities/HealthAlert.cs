namespace MediScope.Common.Models.Entities
{
    public class HealthAlert : BaseEntity
    {
        public Guid HealthMetricId { get; set; }
        public Guid PatientId { get; set; }
        public string AlertType { get; set; } = null!;
        public string Severity { get; set; } = null!;
        public bool IsAcknowledged { get; set; } = false;
        public Guid? AcknowledgedBy { get; set; }
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcknowledgedAt { get; set; }
        public HealthMetric HealthMetric { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public User? AcknowledgedByUser { get; set; }
    }
}