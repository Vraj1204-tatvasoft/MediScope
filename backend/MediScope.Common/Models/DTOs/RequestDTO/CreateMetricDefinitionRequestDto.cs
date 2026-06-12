using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateMetricDefinitionRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string MetricType { get; set; } = null!;   // e.g. blood_pressure

        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; } = null!;  // e.g. Blood Pressure

        [Required]
        [MaxLength(30)]
        public string DefaultUnit { get; set; } = null!;  // e.g. mmHg

        [Range(0, double.MaxValue)]
        public decimal? NormalMin { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? NormalMax { get; set; }

        public string? Description { get; set; }
    }
}