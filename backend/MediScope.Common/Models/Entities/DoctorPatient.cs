using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class DoctorPatient : BaseEntity
    {
        public Guid? DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public ConnectionStatus Status { get; set; } = ConnectionStatus.PendingAdmin;
        public Guid? ReviewedByAdminId { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AdminReviewedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? LastReminderSentAt { get; set; }
        public Doctor? Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
    }
}