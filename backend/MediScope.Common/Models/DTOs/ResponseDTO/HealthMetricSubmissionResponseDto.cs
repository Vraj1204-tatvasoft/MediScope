using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Response
{
    public class HealthMetricSubmissionResponseDto
    {
        public Guid SubmissionId { get; set; }

        public Guid PatientId { get; set; }

        public Guid RecordedByUserId { get; set; }

        public string RecordedByRole { get; set; } = null!;

        public string RecordedByName { get; set; } = null!;

        public DateTime RecordedAt { get; set; }

        public string? Notes { get; set; }

        public Severity Status { get; set; } = Severity.Normal;

        public DateTime CreatedAt { get; set; }

        public List<HealthMetricItemResponseDto> Metrics
        { get; set; } = new();
    }
}