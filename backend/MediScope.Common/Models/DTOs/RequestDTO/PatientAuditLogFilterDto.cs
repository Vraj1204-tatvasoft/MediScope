namespace MediScope.Common.Models.DTOs.Request
{
    public class PatientAuditLogFilterDto
    {
        public Guid? PatientId { get; set; }
        public string? FieldName { get; set; }

        public Guid? ChangedByUserId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}