using System;

namespace MediScope.Common.Models.DTOs.Response
{
    public class AppointmentSummaryDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public string? DoctorNotes { get; set; }
    }
}