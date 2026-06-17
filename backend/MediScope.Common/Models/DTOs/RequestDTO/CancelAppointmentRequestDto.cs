using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class CancelAppointmentRequestDto
    {
        [Required]
        public Guid AppointmentId { get; set; }
        public string? Reason { get; set; }
    }
}