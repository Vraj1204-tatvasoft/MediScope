namespace MediScope.Common.Models.DTOs.Response
{
    /// <summary>
    /// One object per dataset line on the chart.
    /// </summary>
    public class VitalTrendResponseDto
    {
        public string DatasetLabel { get; set; } = null!;

        public string PatientId { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public string MetricType { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Color { get; set; } = null!;   // hex for Chart.js

        public List<VitalTrendPoint> Points { get; set; } = new();
    }
    public class VitalTrendFlatResult
    {
        public Guid patient_id { get; set; }
        public string patient_name { get; set; }
        public string metric_type { get; set; }
        public string unit { get; set; }
        public DateTime recorded_at { get; set; }
        public decimal metric_value { get; set; }
    }
    public class VitalTrendPoint
    {
        public string DateLabel { get; set; } = null!;
        public string DateIso { get; set; } = null!;
        public decimal Value { get; set; }
    }
}