// File: MediScope.Common/Models/DTOs/Response/DoctorDashboardResponseDto.cs
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Response
{
    public class DoctorDashboardResponseDto
    {
        // ── Header ────────────────────────────────────────────────────
        public string DoctorName { get; set; } = null!;
        public string? Specialization { get; set; }            // "Cardiologist"
        public string? Hospital { get; set; }            // "City Heart Institute"

        // ── Critical alert banner ─────────────────────────────────────
        public int CriticalPatientCount { get; set; }
        public string? CriticalPatientNames { get; set; }

        // ── Stat cards ────────────────────────────────────────────────
        public int MyPatients { get; set; }   // total active assigned
        public int ActiveAlerts { get; set; }   // unread critical notifications
        public int TotalRecords { get; set; }   // all submissions across assigned patients
        public int CriticalPatients { get; set; }  // patients with latest submission = CRITICAL

        // ── Latest BP bar chart ───────────────────────────────────────
        public List<PatientBpDto> LatestBpComparison { get; set; } = new();

        // ── Patient Status Overview list ──────────────────────────────
        public List<PatientStatusOverviewDto> PatientStatusOverview { get; set; } = new();

        // ── Recent Patient Activity table ─────────────────────────────
        public List<DoctorRecentActivityDto> RecentActivity { get; set; } = new();
    }

    // ── Latest BP bar chart — one bar pair per patient ──────────────────
    public class PatientBpDto
    {
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = null!;   // first name for chart label
        public decimal LatestSystolic { get; set; }
        public decimal LatestDiastolic { get; set; }
        public string Status { get; set; } = null!;
    }

    // ── Patient Status Overview list row ────────────────────────────────
    public class PatientStatusOverviewDto
    {
        public Guid PatientId { get; set; }
        public string FullName { get; set; } = null!;
        public int TotalRecords { get; set; }
        public int TotalAlerts { get; set; }
        public string LatestStatus { get; set; } = null!;
        public DateTime? LatestRecordAt { get; set; }
    }

    // ── Recent activity table row ────────────────────────────────────────
    public class DoctorRecentActivityDto
    {
        public Guid SubmissionId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public DateTime RecordedAt { get; set; }
        public string AddedBy { get; set; } = null!;
        public string Status { get; set; } = null!;

        // Dynamic metric columns
        public Dictionary<string, string> MetricValues { get; set; } = new();
    }
}