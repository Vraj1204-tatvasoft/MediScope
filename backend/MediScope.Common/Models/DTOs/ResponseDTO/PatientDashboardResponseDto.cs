// File: MediScope.Common/Models/DTOs/Response/PatientDashboardResponseDto.cs

namespace MediScope.Common.Models.DTOs.Response
{
    /// <summary>
    /// Single response object for the patient dashboard.
    /// Designed to be fetched in one call to minimize round trips.
    /// </summary>
    public class PatientDashboardResponseDto
    {
        // ── Header ────────────────────────────────────────────────────
        public string PatientName { get; set; } = null!;
        public string Greeting { get; set; } = null!;
        public int DoctorsConnected { get; set; }

        // ── Health alert banner 
        public int AbnormalReadingCount { get; set; }
        public bool HasHealthAlert => AbnormalReadingCount > 0;

        // ── Latest vital cards (dynamic — one per metric type) ────────
        public List<LatestVitalDto> LatestVitals { get; set; } = new();

        // ── Recent records table (last 5 submissions) ─────────────────
        public List<DashboardRecentRecordDto> RecentRecords { get; set; } = new();

        // ── Trend chart data (last 10 readings per metric) ────────────
        public List<MetricTrendDto> TrendCharts { get; set; } = new();

        // ── My Doctors sidebar widget ─────────────────────────────────
        public List<DashboardDoctorDto> MyDoctors { get; set; } = new();
    }

    // ── Vital card ─────────────────────────────────────────────────────
    public class LatestVitalDto
    {
        public string MetricType { get; set; } = null!;   // "blood_pressure"
        public string DisplayName { get; set; } = null!;   // "Blood Pressure"
        public string DisplayValue { get; set; } = null!;   // "138/88" or "78"
        public string Unit { get; set; } = null!;   // "mmHg", "bpm"

        /// <summary>Normal | Elevated | Critical</summary>
        public string Status { get; set; } = "Normal";

        /// <summary>Percentage change vs previous reading — null if no previous</summary>
        public decimal? TrendPercent { get; set; }

        /// <summary>up | down | flat | null</summary>
        public string? TrendDirection { get; set; }

        public DateTime RecordedAt { get; set; }

        // Normal range from MetricDefinition
        public decimal? NormalMin { get; set; }
        public decimal? NormalMax { get; set; }
    }

    // ── Recent records table row ────────────────────────────────────────
    public class DashboardRecentRecordDto
    {
        public Guid SubmissionId { get; set; }
        public DateTime RecordedAt { get; set; }
        public string AddedBy { get; set; } = null!;
        public string RecordedByRole { get; set; } = null!;
        public string Status { get; set; } = null!;

        /// <summary>Key metric values for the table columns — dynamic</summary>
        public Dictionary<string, string> MetricValues { get; set; } = new();
    }

    // ── Trend chart line ────────────────────────────────────────────────
    public class MetricTrendDto
    {
        public string MetricType { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Unit { get; set; } = null!;

        /// <summary>Last 10 data points oldest → newest</summary>
        public List<TrendDataPoint> DataPoints { get; set; } = new();
    }

    public class TrendDataPoint
    {
        /// <summary>x-axis label: "04-14" format</summary>
        public string DateLabel { get; set; } = null!;
        public decimal Value { get; set; }
    }

    // ── My Doctors sidebar ──────────────────────────────────────────────
    public class DashboardDoctorDto
    {
        public Guid? DoctorId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Specialization { get; set; }
        public bool IsActive { get; set; }
    }
}