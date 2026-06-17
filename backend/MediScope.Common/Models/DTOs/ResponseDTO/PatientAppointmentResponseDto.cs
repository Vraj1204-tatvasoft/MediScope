namespace MediScope.Common.Models.DTOs.Response
{
    public class PatientAppointmentResponseDto
    {
        public Guid AppointmentId { get; set; }
        public Guid SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = null!;

        // Doctor info
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string? Specialization { get; set; }
        public string? Hospital { get; set; }

        // Notes
        public string? DoctorNotes { get; set; }
        public string? PatientNotes { get; set; }
        public DateTime? RescheduledTo { get; set; }
        public string? RescheduleReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}