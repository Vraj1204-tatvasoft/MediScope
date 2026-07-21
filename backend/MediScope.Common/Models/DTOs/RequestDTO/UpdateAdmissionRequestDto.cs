using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateAdmissionRequestDto
    {
        public Guid DoctorId { get; set; }

        public string AdmissionReason { get; set; } = string.Empty;

        public DateTime AdmissionDate { get; set; }

        public DateTime? ExpectedDischargeDate { get; set; }

        public string? Remarks { get; set; }
    }
}