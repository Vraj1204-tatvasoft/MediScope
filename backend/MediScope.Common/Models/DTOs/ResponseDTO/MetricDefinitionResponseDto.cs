namespace MediScope.Common.Models.DTOs.Response
{
    public class MetricDefinitionResponseDto
    {
        public Guid Id { get; set; }
        public string MetricType { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string DefaultUnit { get; set; } = null!;
        public decimal? NormalMin { get; set; }
        public decimal? NormalMax { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? NormalRangeDisplay =>
            NormalMin.HasValue && NormalMax.HasValue
                ? $"{NormalMin} - {NormalMax} {DefaultUnit}"
                : null;
    }
}