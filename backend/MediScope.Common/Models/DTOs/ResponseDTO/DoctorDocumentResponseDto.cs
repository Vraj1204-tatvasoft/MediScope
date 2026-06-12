namespace MediScope.Common.Models.DTOs.Response
{
    public class DoctorDocumentResponseDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsViewedByDoctor { get; set; }
        public bool IsReviewed { get; set; }
        public string? Feedback { get; set; }
        public string? Severity { get; set; }
    }
}