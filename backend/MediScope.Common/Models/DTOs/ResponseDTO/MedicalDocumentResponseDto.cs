namespace MediScope.Common.Models.DTOs.Response
{
    public class MedicalDocumentResponseDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string DoctorName { get; set; } = null!;
        public bool IsViewedByDoctor { get; set; }
        public bool IsReviewed { get; set; }
        public string? Feedback { get; set; }
        public string? Severity { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}