using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class PatientAdmission : BaseEntity
    {
        public string AdmissionNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid WardId { get; set; }
        public Guid RoomId { get; set; }
        public Guid BedId { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string AdmissionReason { get; set; } = string.Empty;
        public DateTime? ExpectedDischargeDate { get; set; }
        public DateTime? ActualDischargeDate { get; set; }
        public string? DischargeNotes { get; set; }
        public string? Remarks { get; set; }
        public AdmissionStatus Status { get; set; } = AdmissionStatus.Active;
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public Ward Ward { get; set; } = null!;
        public Room Room { get; set; } = null!;
        public Bed Bed { get; set; } = null!;
        public ICollection<BedTransferHistory> TransferHistory { get; set; } = new List<BedTransferHistory>();
    }
}