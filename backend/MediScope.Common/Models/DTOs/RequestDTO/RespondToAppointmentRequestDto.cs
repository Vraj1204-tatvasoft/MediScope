using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{

    public class RespondToAppointmentRequestDto
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        public string Action { get; set; } = null!;

        public string? PatientNotes { get; set; }
        public DateTime? RescheduledTo { get; set; }
        public string? RescheduleReason { get; set; }
    }
}