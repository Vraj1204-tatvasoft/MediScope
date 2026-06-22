using System;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Appointment : BaseEntity
    {
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? DoctorNotes { get; set; }
        public string? PatientNotes { get; set; }
        public Guid? RescheduleRequestedBy { get; set; }
        public DateTime? RescheduledTo { get; set; }
        public string? RescheduleReason { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();
    }
}