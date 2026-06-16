// File: MediScope.Business/Services/AdminDashboardService.cs

using Microsoft.EntityFrameworkCore;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Enums;

namespace MediScope.Business.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _uow;

        public AdminDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AdminDashboardResponseDto> GetDashboardAsync(Guid adminUserId)
        {
            var patients = (await _uow.Patients.GetAllAsync()).ToList();
            var doctors = (await _uow.Doctors.GetAllWithUserAsync()).ToList();

            // ── 1. Fetch flat metrics ──────────────────────────────────
            var rawMetrics = (await _uow.HealthMetrics.GetAllWithMetricsAsync()).ToList();

            //  2. Group them immediately into distinct batches 
            var groupedSubmissions = rawMetrics
                .GroupBy(m => m.SubmissionId)
                .ToList();

            var doctorPatients = await _uow.DoctorPatients
                .GetAllWithDetailsQueryable()
                .ToListAsync();

            var unreadAlerts = await _uow.Notifications.GetUnreadCountAsync(adminUserId);

            // ── 3. Stat cards (Using grouped batches) ──────────────────
            var activeAlerts = groupedSubmissions.Count(g =>
                g.First().Status == Severity.Elevated || g.First().Status == Severity.Critical);

            var stats = new AdminStatsDto
            {
                TotalPatients = patients.Count,
                TotalDoctors = doctors.Count,
                TotalRecords = groupedSubmissions.Count, // ── 🛠️ FIX: Count batches
                ActiveAlerts = activeAlerts,
            };

            // ── 4. Reading summary (Using grouped batches) ─────────────
            var normalCount = groupedSubmissions.Count(g => g.First().Status == Severity.Normal);
            var elevatedCount = groupedSubmissions.Count(g => g.First().Status == Severity.Elevated);
            var criticalCount = groupedSubmissions.Count(g => g.First().Status == Severity.Critical);

            var readingSummary = new ReadingSummaryDto
            {
                NormalCount = normalCount,
                ElevatedCount = elevatedCount,
                CriticalCount = criticalCount,
                Total = groupedSubmissions.Count,
            };

            // ── 5. Reading severity donut ──────────────────────────────
            var severityDonut = new SeverityDonutDto
            {
                Normal = normalCount,
                Elevated = elevatedCount,
                Critical = criticalCount,
            };

            // ── 6. Platform growth ─────────────────────────────────────
            var platformGrowth = BuildPlatformGrowth(patients, doctors);

            // ── 7. Alerts by metric type (STILL USING RAW METRICS) ─────
            // This stays flat because we specifically want to know *which* vital signs triggered alerts
            var alertsByMetric = BuildAlertsByMetric(rawMetrics);

            // ── 8. Doctor patient load ─────────────────────────────────
            var doctorLoad = doctors
                .Select(d => new DoctorLoadDto
                {
                    DoctorId = d.Id,
                    FullName = d.User?.FullName ?? string.Empty,
                    DoctorName = d.User?.FullName?.Split(' ').LastOrDefault() ?? string.Empty,
                    Specialization = d.Specialization,
                    ActivePatients = doctorPatients.Count(dp =>
                        dp.DoctorId == d.Id &&
                        dp.Status == ConnectionStatus.Active &&
                        !dp.IsDeleted),
                })
                .OrderByDescending(d => d.ActivePatients)
                .ToList();

            // ── 9. Recent activity table ───────────────────────────────
            var recentActivity = groupedSubmissions
                .OrderByDescending(g => g.First().RecordedAt)
                .Take(10)
                .Select(g =>
                {
                    var first = g.First();

                    return new AdminRecentActivityDto
                    {
                        SubmissionId = first.SubmissionId,
                        PatientName = first.Patient?.User?.FullName ?? "Unknown",
                        RecordedAt = first.RecordedAt,
                        AddedBy = first.RecordedByRole == "Patient" ? "Patient" : "Doctor",
                        Status = first.Status.ToString(),
                        MetricValues = BuildMetricValues(g.ToList()),
                    };
                })
                .ToList();

            return new AdminDashboardResponseDto
            {
                Stats = stats,
                ReadingSummary = readingSummary,
                PlatformGrowth = platformGrowth,
                ReadingSeverity = severityDonut,
                AlertsByMetric = alertsByMetric,
                DoctorLoad = doctorLoad,
                RecentActivity = recentActivity,
                UnreadAlertCount = unreadAlerts,
            };
        }

        // ── Platform Growth ────────────────────────────────────────────
        private static List<GrowthDataPointDto> BuildPlatformGrowth(
            List<Patient> patients,
            List<Doctor> doctors)
        {
            var result = new List<GrowthDataPointDto>();
            var today = DateTime.UtcNow.Date;

            var daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
            var currentWeekMonday = today.AddDays(-daysFromMonday);

            for (int i = 7; i >= 0; i--)
            {
                var weekStart = currentWeekMonday.AddDays(-i * 7);
                var weekEnd = weekStart.AddDays(7).AddSeconds(-1);

                result.Add(new GrowthDataPointDto
                {
                    MonthLabel = weekStart.ToString("MMM dd"),
                    PatientCount = patients.Count(p =>
                        p.CreatedAt.Date <= weekEnd.Date &&
                        !p.IsDeleted),
                    DoctorCount = doctors.Count(d =>
                        d.CreatedAt.Date <= weekEnd.Date &&
                        !d.IsDeleted),
                });
            }

            return result;
        }

        // ── Alerts by Metric Type ──────────────────────────────────────
        private static List<AlertByMetricDto> BuildAlertsByMetric(
            List<HealthMetric> metrics)
        {
            return metrics
                .Where(m => m.Status != Severity.Normal)
                .Where(m => m.MetricDefinition != null)
                .Where(m =>
                    (m.MetricDefinition!.NormalMax.HasValue && m.Value > m.MetricDefinition.NormalMax) ||
                    (m.MetricDefinition!.NormalMin.HasValue && m.Value < m.MetricDefinition.NormalMin))
                .GroupBy(m => new
                {
                    m.MetricType,
                    DisplayName = m.MetricDefinition?.DisplayName ?? m.MetricType
                })
                .Select(g => new AlertByMetricDto
                {
                    MetricType = g.Key.MetricType,
                    DisplayName = g.Key.DisplayName,
                    AbnormalCount = g.Count(),
                })
                .OrderByDescending(a => a.AbnormalCount)
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