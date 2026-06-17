using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateAppointmentRequestDto
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [Range(10, 120, ErrorMessage = "Duration must be between 10 and 120 minutes.")]
        public int DurationMinutes { get; set; }

        public string? DoctorNotes { get; set; }
    }
}