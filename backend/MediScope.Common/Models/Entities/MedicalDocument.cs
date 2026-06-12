namespace MediScope.Common.Models.Entities
{
    public class MedicalDocument : BaseEntity
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }

        public string FileName { get; set; } = null!;
        public string StoredName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSizeBytes { get; set; }

        public string? Description { get; set; }
        public string? Category { get; set; }

        public bool IsViewedByDoctor { get; set; }

        public bool IsReviewed { get; set; }

        public string? Feedback { get; set; }

        public string? Severity { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}