using System;
using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class RescheduleAppointmentRequestDto
    {
        [Required]
        public Guid AppointmentId { get; set; }
        [Required]
        public DateTime RescheduledTo { get; set; }
        public string? RescheduleReason { get; set; }
    }
}