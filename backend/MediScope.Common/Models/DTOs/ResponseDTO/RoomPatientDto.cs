using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Response
{
    public class RoomPatientDto
    {
        public Guid AdmissionId { get; set; }

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = "";

        public string DoctorName { get; set; } = "";

        public DateTime AdmissionDate { get; set; }

        public DateTime? ExpectedDischargeDate { get; set; }

        public string AdmissionReason { get; set; } = "";

        public AdmissionStatus Status { get; set; }
    }
}