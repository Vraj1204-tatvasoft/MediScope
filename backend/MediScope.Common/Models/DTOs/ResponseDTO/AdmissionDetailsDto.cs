namespace MediScope.Common.Models.DTOs.Response
{
    public class AdmissionDetailsDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public Guid WardId { get; set; }
        public string WardName { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public Guid BedId { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public string AdmissionReason { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public DateTime? ExpectedDischargeDate { get; set; }
        public string? Remarks { get; set; }
    }
}