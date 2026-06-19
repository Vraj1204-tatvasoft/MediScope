using System;

namespace MediScope.Common.Models.DTOs.Response
{
    public class DoctorAppointmentResponseDto
    {
        public Guid AppointmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = null!;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public string? DoctorNotes { get; set; }
        public string? PatientNotes { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? RescheduleRequestedBy { get; set; }
    }
}