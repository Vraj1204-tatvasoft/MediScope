namespace MediScope.Common.Models.DTOs.Response
{
    public class HealthMetricItemResponseDto
    {
        public Guid Id { get; set; }

        public string MetricType { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public decimal Value { get; set; }

        public string Unit { get; set; } = null!;

        public decimal? NormalMin { get; set; }

        public decimal? NormalMax { get; set; }

        public string Status =>
            NormalMin.HasValue &&
            NormalMax.HasValue

                ? Value < NormalMin
                    ? "LOW"

                : Value > NormalMax
                    ? "HIGH"

                : "NORMAL"

                : "UNKNOWN";
    }
}