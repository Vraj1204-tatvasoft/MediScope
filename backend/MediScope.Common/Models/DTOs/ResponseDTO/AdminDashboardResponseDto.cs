// File: MediScope.Common/Models/DTOs/Response/AdminDashboardResponseDto.cs

namespace MediScope.Common.Models.DTOs.Response
{
    public class AdminDashboardResponseDto
    {
        // ── Top stat cards ─────────────────────────────────────────────
        public AdminStatsDto Stats { get; set; } = new();

        // ── Normal / Elevated / Critical blocks ────────────────────────
        public ReadingSummaryDto ReadingSummary { get; set; } = new();

        // ── Platform Growth line chart ─────────────────────────────────
        public List<GrowthDataPointDto> PlatformGrowth { get; set; } = new();

        // ── Reading Severity donut chart ───────────────────────────────
        public SeverityDonutDto ReadingSeverity { get; set; } = new();

        // ── Alerts by Metric Type bar chart ───────────────────────────
        public List<AlertByMetricDto> AlertsByMetric { get; set; } = new();

        // ── Doctor Patient Load bar chart ──────────────────────────────
        public List<DoctorLoadDto> DoctorLoad { get; set; } = new();

        // ── Recent Health Activity table ───────────────────────────────
        public List<AdminRecentActivityDto> RecentActivity { get; set; } = new();

        // ── Unread alert count (top right badge) ──────────────────────
        public int UnreadAlertCount { get; set; }
    }

    // ── Stat cards ─────────────────────────────────────────────────────
    public class AdminStatsDto
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalRecords { get; set; }   // total health metric submissions
        public int ActiveAlerts { get; set; }   // submissions with status != NORMAL
    }

    // ── Reading summary blocks ──────────────────────────────────────────
    public class ReadingSummaryDto
    {
        public int NormalCount { get; set; }
        public int ElevatedCount { get; set; }
        public int CriticalCount { get; set; }
        public int Total { get; set; }
        public decimal NormalPct => Total > 0 ? Math.Round((decimal)NormalCount / Total * 100, 1) : 0;
        public decimal ElevatedPct => Total > 0 ? Math.Round((decimal)ElevatedCount / Total * 100, 1) : 0;
        public decimal CriticalPct => Total > 0 ? Math.Round((decimal)CriticalCount / Total * 100, 1) : 0;
    }

    // ── Platform Growth line chart ──────────────────────────────────────
    public class GrowthDataPointDto
    {
        public string MonthLabel { get; set; } = null!;   // "Jan", "Feb" etc.
        public int PatientCount { get; set; }
        public int DoctorCount { get; set; }
    }

    // ── Reading Severity donut ──────────────────────────────────────────
    public class SeverityDonutDto
    {
        public int Normal { get; set; }
        public int Elevated { get; set; }
        public int Critical { get; set; }
    }

    // ── Alerts by metric type bar chart ────────────────────────────────
    public class AlertByMetricDto
    {
        public string MetricType { get; set; } = null!;   // "blood_pressure"
        public string DisplayName { get; set; } = null!;   // "Blood Pressure"
        public int AbnormalCount { get; set; }
    }

    // ── Doctor patient load bar chart ───────────────────────────────────
    public class DoctorLoadDto
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;   // last name for chart label
        public string FullName { get; set; } = null!;
        public string? Specialization { get; set; }
        public int ActivePatients { get; set; }
    }

    // ── Recent health activity table row ───────────────────────────────
    public class AdminRecentActivityDto
    {
        public Guid SubmissionId { get; set; }
        public string PatientName { get; set; } = null!;
        public DateTime RecordedAt { get; set; }
        public string AddedBy { get; set; } = null!;   // "Patient" | "Doctor"
        public string Status { get; set; } = null!;   // NORMAL|ELEVATED|CRITICAL

        // Dynamic metric columns for the table — same approach as health history
        public Dictionary<string, string> MetricValues { get; set; } = new();
    }
}