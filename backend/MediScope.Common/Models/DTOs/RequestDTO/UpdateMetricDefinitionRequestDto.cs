using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateMetricDefinitionRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string DefaultUnit { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal? NormalMin { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? NormalMax { get; set; }

        public string? Description { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (NormalMin.HasValue &&
                NormalMax.HasValue &&
                NormalMax < NormalMin)
            {
                yield return new ValidationResult(
                    "Normal max value cannot be smaller than normal min value.",
                    new[]
                    {
                        nameof(NormalMax)
                    });
            }
        }
    }
}