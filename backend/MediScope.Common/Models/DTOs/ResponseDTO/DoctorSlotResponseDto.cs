namespace MediScope.Common.Models.DTOs.Response
{
    public class DoctorSlotResponseDto
    {
        public Guid SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string SlotStatus { get; set; } = null!;
        public string? Notes { get; set; }

        // Populated when slot is booked
        public Guid? AppointmentId { get; set; }
        public string? AppointmentStatus { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
    }
}