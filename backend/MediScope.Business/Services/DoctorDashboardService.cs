// File: MediScope.Business/Services/DoctorDashboardService.cs

using Microsoft.EntityFrameworkCore;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class DoctorDashboardService : IDoctorDashboardService
    {
        private readonly IUnitOfWork _uow;

        public DoctorDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<DoctorDashboardResponseDto> GetDashboardAsync(Guid doctorUserId)
        {
            // 1. Get doctor profile
            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            // 2. Get all active patient links for this doctor
            var activeLinks = await _uow.DoctorPatients
                .GetAllWithDetailsQueryable()
                .Where(dp => dp.DoctorId == doctor.Id &&
                             dp.Status == "active" &&
                             !dp.IsDeleted)
                .ToListAsync();

            var assignedPatientIds = activeLinks
                .Select(dp => dp.PatientId)
                .ToHashSet();

            if (assignedPatientIds.Count == 0)
            {
                // Doctor has no patients yet — return empty dashboard
                return new DoctorDashboardResponseDto
                {
                    DoctorName = doctor.User?.FullName ?? string.Empty,
                    Specialization = doctor.Specialization,
                    Hospital = doctor.Hospital,
                };
            }

            // 3. Load all metrics for assigned patients directly from the flat table
            var patientMetricsResult = await _uow.HealthMetrics
                .FindAsync(m => assignedPatientIds.Contains(m.PatientId) && !m.IsDeleted);

            var patientMetrics = patientMetricsResult.ToList();

            // Calculate distinct submissions using our SubmissionId grouping tag
            int totalEncounters = patientMetrics.Select(m => m.SubmissionId).Distinct().Count();

            // 4. Unread critical notifications for this doctor
            var activeAlerts = await _uow.Notifications.GetUnreadCountAsync(doctorUserId);

            // ── Build each section ─────────────────────────────────────

            // Note: We now pass activeLinks into RecentActivity so we can look up the patient name efficiently
            var criticalInfo = BuildCriticalAlertBanner(activeLinks, patientMetrics);
            var patientOverview = BuildPatientStatusOverview(activeLinks, patientMetrics);
            var recentActivity = BuildRecentActivity(patientMetrics, activeLinks);
            var criticalPatients = patientOverview.Count(p => p.LatestStatus == "CRITICAL");

            return new DoctorDashboardResponseDto
            {
                DoctorName = doctor.User?.FullName ?? string.Empty,
                Specialization = doctor.Specialization,
                Hospital = doctor.Hospital,
                CriticalPatientCount = criticalInfo.Count,
                CriticalPatientNames = criticalInfo.Names,
                MyPatients = assignedPatientIds.Count,
                ActiveAlerts = activeAlerts,
                TotalRecords = totalEncounters,
                CriticalPatients = criticalPatients,
                PatientStatusOverview = patientOverview,
                RecentActivity = recentActivity,
            };
        }

        // ── Critical Alert Banner ──────────────────────────────────────
        private static (int Count, string? Names) BuildCriticalAlertBanner(
            List<DoctorPatient> links,
            List<HealthMetric> metrics)
        {
            var criticalPatients = links
                .Select(dp =>
                {
                    // Because flattened metrics in a batch all share the exact same RecordedAt and Status, 
                    // we just grab the single newest metric row for this patient to determine their latest status.
                    var latest = metrics
                        .Where(s => s.PatientId == dp.PatientId)
                        .OrderByDescending(s => s.RecordedAt)
                        .FirstOrDefault();

                    return new { dp.Patient, LatestStatus = latest?.Status };
                })
                .Where(x => x.LatestStatus == "CRITICAL")
                .ToList();

            if (criticalPatients.Count == 0) return (0, null);

            var names = string.Join(", ",
                criticalPatients
                    .Take(3)
                    .Select(x => x.Patient?.User?.FullName ?? "Unknown"));

            if (criticalPatients.Count > 3)
                names += $" and {criticalPatients.Count - 3} more";

            return (criticalPatients.Count, names);
        }

        public async Task<List<VitalTrendResponseDto>> GetVitalTrendsAsync(Guid doctorUserId, string metricType, string patientId, string duration, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;
            var endOfToday = now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            var start = duration.ToLower() switch
            {
                "last_week" => now.AddDays(-7),
                "last_month" => now.AddMonths(-1),
                "last_6months" => now.AddMonths(-6),
                "last_year" => now.AddYears(-1),
                "custom" => fromDate ?? now.AddMonths(-1),
                _ => now.AddMonths(-1),
            };
            var end = (duration.ToLower() == "custom" && toDate.HasValue) ? toDate.Value : endOfToday;

            var rawData = await _uow.DoctorDashboard.CallVitalTrendsFunctionAsync(doctorUserId, metricType, patientId, start, end);

            var result = new List<VitalTrendResponseDto>();
            if (!rawData.Any()) return result;

            var colors = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#06b6d4" };
            int colorIdx = 0;

            var groupedByPatientAndMetric = rawData.GroupBy(r => new { r.patient_id, r.patient_name, r.metric_type, r.unit });

            foreach (var group in groupedByPatientAndMetric)
            {
                var shortName = group.Key.patient_name.Split(' ')[0];

                var displayName = group.Key.metric_type.Replace("_", " ").ToUpper();
                if (group.Key.metric_type == "systolic_blood_pressure") displayName = "Systolic BP";
                if (group.Key.metric_type.Contains("diastolic")) displayName = "Diastolic BP";

                result.Add(new VitalTrendResponseDto
                {
                    DatasetLabel = patientId == "all" ? $"{shortName} - {displayName}" : displayName,
                    PatientId = group.Key.patient_id.ToString(),
                    PatientName = group.Key.patient_name,
                    MetricType = group.Key.metric_type,
                    DisplayName = displayName,
                    Unit = group.Key.unit,
                    Color = colors[colorIdx++ % colors.Length],
                    Points = group.Select(row => new VitalTrendPoint
                    {
                        DateIso = row.recorded_at.ToString("o"),
                        DateLabel = row.recorded_at.ToString("MMM dd"),
                        Value = row.metric_value
                    }).ToList()
                });
            }

            return result;
        }

        // ── Patient Status Overview ────────────────────────────────────
        private static List<PatientStatusOverviewDto> BuildPatientStatusOverview(
            List<DoctorPatient> links,
            List<HealthMetric> metrics)
        {
            return links
                .Select(dp =>
                {
                    var patient = dp.Patient;

                    // Group the flat rows back into batches using SubmissionId
                    var patientBatches = metrics
                        .Where(m => m.PatientId == patient.Id)
                        .GroupBy(m => m.SubmissionId)
                        .ToList();

                    // Find the most recent batch
                    var latestBatch = patientBatches
                        .OrderByDescending(g => g.First().RecordedAt)
                        .FirstOrDefault();

                    var alertCount = patientBatches.Count(g =>
                        g.First().Status == "ELEVATED" || g.First().Status == "CRITICAL");

                    return new PatientStatusOverviewDto
                    {
                        PatientId = patient.Id,
                        FullName = patient.User?.FullName ?? string.Empty,
                        TotalRecords = patientBatches.Count,
                        TotalAlerts = alertCount,
                        LatestStatus = latestBatch?.First().Status ?? "NORMAL",
                        LatestRecordAt = latestBatch?.First().RecordedAt ?? dp.AssignedAt,
                    };
                })
                .OrderBy(p => p.LatestStatus switch
                {
                    "CRITICAL" => 0,
                    "ELEVATED" => 1,
                    _ => 2
                })
                .ThenByDescending(p => p.LatestRecordAt)
                .ToList();
        }

        // ── Recent Patient Activity Table ──────────────────────────────
        private static List<DoctorRecentActivityDto> BuildRecentActivity(
            List<HealthMetric> metrics,
            List<DoctorPatient> links)
        {
            return metrics
                .GroupBy(m => m.SubmissionId)
                .OrderByDescending(g => g.First().RecordedAt)
                .Take(10)
                .Select(g =>
                {
                    var first = g.First();
                    // Grab the patient's name directly from our DoctorPatient relationships
                    var patientName = links.FirstOrDefault(l => l.PatientId == first.PatientId)?.Patient?.User?.FullName ?? "Unknown";

                    return new DoctorRecentActivityDto
                    {
                        SubmissionId = first.SubmissionId,
                        PatientId = first.PatientId,
                        PatientName = patientName,
                        RecordedAt = first.RecordedAt,
                        AddedBy = first.RecordedByRole == "Patient" ? "Patient" : "Doctor",
                        Status = first.Status,
                        MetricValues = BuildMetricValues(g.ToList()),
                    };
                })
                .ToList();
        }

        // ── Metric values for table row ────────────────────────────────
        private static Dictionary<string, string> BuildMetricValues(
            List<HealthMetric> metrics)
        {
            var result = new Dictionary<string, string>();

            var systolic = metrics.FirstOrDefault(m =>
                m.MetricType.ToLower() == "systolic_blood_pressure");
            var diastolic = metrics.FirstOrDefault(m =>
                m.MetricType.ToLower() == "diastolic_blood_pressure");

            if (systolic != null || diastolic != null)
            {
                result["bp"] =
                    $"{(systolic != null ? (int)systolic.Value : (object)"—")}/" +
                    $"{(diastolic != null ? (int)diastolic.Value : (object)"—")}";
            }

            foreach (var m in metrics)
            {
                var key = m.MetricType.ToLower();
                if (key == "systolic_blood_pressure" ||
                    key == "diastolic_blood_pressure") continue;

                result[key] = key switch
                {
                    "heart_rate" => $"{(int)m.Value}",
                    "o2_saturation" => $"{(int)m.Value}%",
                    "sleep" => $"{m.Value} hrs",
                    _ => $"{m.Value} {m.Unit}",
                };
            }

            return result;
        }
    }
}