using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Request
{
    public class AddHealthMetricRequestDto
    {
        public Guid? SubmissionId { get; set; }
        [Required]
        public DateTime RecordedAt { get; set; }

        public string? Notes { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? AppointmentId { get; set; }
        [Required]
        [MinLength(1)]
        public List<AddMetricValueRequestDto> Metrics { get; set; }
            = new();
    }

    public class AddMetricValueRequestDto
    {
        [Required]
        public Guid MetricDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MetricType { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "Value must be greater than 0.")]
        public decimal Value { get; set; }

        [Required]
        [MaxLength(30)]
        public string Unit { get; set; } = null!;
    }
}