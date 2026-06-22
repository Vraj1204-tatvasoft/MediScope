using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class HealthMetric : BaseEntity
    {
        public Guid SubmissionId { get; set; }
        public Guid? AppointmentId { get; set; }
        public string MetricType { get; set; } = null!;
        public decimal Value { get; set; }
        public string Unit { get; set; } = null!;
        public Guid PatientId { get; set; }
        public Guid RecordedByUserId { get; set; }
        public string RecordedByRole { get; set; } = null!;
        public DateTime RecordedAt { get; set; }
        public string? Notes { get; set; }
        public Severity Status { get; set; } = Severity.Normal;
        // Navigation
        public Patient Patient { get; set; } = null!;
        public User RecordedByUser { get; set; } = null!;
        public MetricDefinition MetricDefinition { get; set; } = null!;
        public Appointment? Appointment { get; set; }
        public ICollection<HealthAlert> HealthAlerts { get; set; } = new List<HealthAlert>();
    }
}