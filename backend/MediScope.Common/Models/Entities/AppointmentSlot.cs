using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.Entities
{
    public class AppointmentSlot : BaseEntity
    {
        public Guid DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public AppointmentSlotStatus Status { get; set; } = AppointmentSlotStatus.Available;
        public string? Notes { get; set; }
        // Navigation
        public Doctor Doctor { get; set; } = null!;
        public Appointment? Appointment { get; set; }
    }
}