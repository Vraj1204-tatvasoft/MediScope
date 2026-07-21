using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Request
{
    public class AdmitPatientRequestDto
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid WardId { get; set; }
        public Guid RoomId { get; set; }
        public Guid BedId { get; set; }
        public string AdmissionReason { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public DateTime? ExpectedDischargeDate { get; set; }
        public string? Remarks { get; set; }
    }
}