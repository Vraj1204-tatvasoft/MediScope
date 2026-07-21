using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class BedTransferHistory : BaseEntity
    {
        public Guid AdmissionId { get; set; }
        public Guid FromWardId { get; set; }
        public Guid FromRoomId { get; set; }
        public Guid FromBedId { get; set; }
        public Guid ToWardId { get; set; }
        public Guid ToRoomId { get; set; }
        public Guid ToBedId { get; set; }
        public DateTime TransferDate { get; set; }
        public string? TransferReason { get; set; }
        public PatientAdmission Admission { get; set; } = null!;
        public Ward FromWard { get; set; } = null!;
        public Room FromRoom { get; set; } = null!;
        public Bed FromBed { get; set; } = null!;
        public Ward ToWard { get; set; } = null!;
        public Room ToRoom { get; set; } = null!;
        public Bed ToBed { get; set; } = null!;
    }
}