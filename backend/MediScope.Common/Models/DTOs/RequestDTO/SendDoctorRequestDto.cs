
using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class SendDoctorRequestDto
    {
        public Guid? DoctorId { get; set; }
    }
    public class AdminApproveRequestDto
    {
        [Required]
        public Guid DoctorPatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        public string? AdminNote { get; set; }
    }
    public class AdminRejectRequestDto
    {
        [Required]
        public Guid DoctorPatientId { get; set; }

        [MaxLength(500)]
        public string? AdminNote { get; set; }
    }
    public class RespondToRequestDto
    {
        [Required]
        public Guid DoctorPatientId { get; set; }

        [Required]
        public bool Accept { get; set; }
    }

    public class RevokeAccessDto
    {
        [Required]
        public Guid DoctorPatientId { get; set; }
    }
}