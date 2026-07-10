using System;

namespace MediScope.Common.Models.DTOs.Response
{
    public class DoctorAvailabilityResponseDto
    {
        public Guid AppointmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = null!;
    }
}