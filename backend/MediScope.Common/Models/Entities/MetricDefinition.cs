namespace MediScope.Common.Models.Entities
{
    public class MetricDefinition : BaseEntity
    {
        public string MetricType { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string DefaultUnit { get; set; } = null!;
        public decimal? NormalMin { get; set; }
        public decimal? NormalMax { get; set; }
        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();
    }
}