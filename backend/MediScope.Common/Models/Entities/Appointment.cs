using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.Entities
{
    public class Appointment : BaseEntity
    {
        public Guid SlotId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? DoctorNotes { get; set; }
        public string? PatientNotes { get; set; }
        public DateTime? RescheduledTo { get; set; }
        public string? RescheduleReason { get; set; }

        // Navigation
        public AppointmentSlot Slot { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
    }
}