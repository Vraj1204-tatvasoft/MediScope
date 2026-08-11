namespace MediScope.Common.Models.DTOs.Response
{
    public class PatientAuditLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
        public string FieldName { get; set; } = null!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}