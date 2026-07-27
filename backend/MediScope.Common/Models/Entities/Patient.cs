using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Patient : BaseEntity
    {
        public Guid UserId { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        public String? BloodGroup { get; set; }

        public string? ContactNumber { get; set; }

        public string? Address { get; set; }

        public bool ConsentProfileVisible { get; set; } = false;
        public string? RazorpayCustomerId { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();
        public ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();
        public ICollection<HealthAlert> HealthAlerts { get; set; } = new List<HealthAlert>();
        public ICollection<PatientAuditLog> AuditLogs { get; set; } = new List<PatientAuditLog>();
        public ICollection<MedicalDocument> MedicalDocuments { get; set; } = new List<MedicalDocument>();
        public ICollection<PatientAdmission> PatientAdmissions { get; set; } = new List<PatientAdmission>();
        public ICollection<QuestionnaireSubmission> QuestionnaireSubmissions { get; set; } = new List<QuestionnaireSubmission>();
    }
}