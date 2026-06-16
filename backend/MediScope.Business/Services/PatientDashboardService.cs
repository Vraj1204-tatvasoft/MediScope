using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Enums;

namespace MediScope.Business.Services
{
    public class PatientDashboardService : IPatientDashboardService
    {
        private readonly IUnitOfWork _uow;

        public PatientDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PatientDashboardResponseDto> GetDashboardAsync(Guid userId)
        {
            var patient = await _uow.PatientDashboard
                .GetDashboardDataAsync(userId)
                ?? throw new Exception("Patient not found.");

            // Group the flat metrics into events
            var submissions = patient.HealthMetrics
                .GroupBy(m => m.SubmissionId)
                .OrderByDescending(g => g.First().RecordedAt)
                .ToList();

            // GREETING
            var hour = DateTime.Now.Hour;
            var greeting = hour < 12
                ? "Good Morning"
                : hour < 18
                    ? "Good Afternoon"
                    : "Good Evening";

            // ALERT COUNT
            var abnormalCount = submissions
                .Take(3)
                .Count(g => g.First().Status != Severity.Normal);

            // LATEST VITALS
            var latestVitals = submissions
                .SelectMany(g => g)
                .GroupBy(m => m.MetricType)
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.RecordedAt).First();
                    var previous = g.OrderByDescending(x => x.RecordedAt).Skip(1).FirstOrDefault();

                    decimal? trend = null;
                    string? direction = null;

                    if (previous != null && previous.Value != 0)
                    {
                        trend = Math.Round(((latest.Value - previous.Value) / previous.Value) * 100, 1);
                        direction = trend > 0 ? "up" : trend < 0 ? "down" : "flat";
                    }

                    return new LatestVitalDto
                    {
                        MetricType = latest.MetricType,
                        DisplayName = latest.MetricDefinition?.DisplayName ?? latest.MetricType,
                        DisplayValue = latest.Value.ToString(),
                        Unit = latest.Unit,
                        Status = latest.Value > (latest.MetricDefinition?.NormalMax ?? decimal.MaxValue) ||
                                 latest.Value < (latest.MetricDefinition?.NormalMin ?? decimal.MinValue)
                                    ? Severity.Critical : Severity.Normal,
                        TrendPercent = trend,
                        TrendDirection = direction,
                        RecordedAt = latest.RecordedAt, // Updated to RecordedAt
                        NormalMin = latest.MetricDefinition?.NormalMin,
                        NormalMax = latest.MetricDefinition?.NormalMax
                    };
                })
                .ToList();

            // RECENT RECORDS
            var recentRecords = submissions
                .Take(5)
                .Select(g =>
                {
                    var first = g.First();
                    return new DashboardRecentRecordDto
                    {
                        SubmissionId = first.SubmissionId, // or g.Key
                        RecordedAt = first.RecordedAt,
                        AddedBy = first.RecordedByUser?.FullName ?? "Unknown",
                        RecordedByRole = first.RecordedByRole,
                        Status = first.Status.ToString(),
                        MetricValues = g.ToDictionary(m => m.MetricType, m => $"{m.Value} {m.Unit}")
                    };
                })
                .ToList();

            // TREND CHARTS
            var trendCharts = submissions
                .SelectMany(g => g)
                .GroupBy(m => m.MetricType)
                .Select(group =>
                    new MetricTrendDto
                    {
                        MetricType = group.Key,
                        DisplayName = group.First().MetricDefinition?.DisplayName ?? group.Key,
                        Unit = group.First().Unit,
                        DataPoints = group
                            .OrderBy(x => x.RecordedAt)
                            .TakeLast(10)
                            .Select(x =>
                                new TrendDataPoint
                                {
                                    DateLabel = x.RecordedAt.ToString("MM-dd"),
                                    Value = x.Value
                                })
                            .ToList()
                    })
                .ToList();

            // DOCTORS
            var doctors = patient.DoctorPatients
                .Where(dp => dp.Status == ConnectionStatus.Active)
                .Select(dp =>
                    new DashboardDoctorDto
                    {
                        DoctorId = dp.DoctorId,
                        FullName = dp.Doctor.User.FullName,
                        Specialization = dp.Doctor.Specialization,
                        IsActive = true
                    })
                .ToList();

            return new PatientDashboardResponseDto
            {
                PatientName = patient.User.FullName,
                Greeting = greeting,
                DoctorsConnected = doctors.Count,
                AbnormalReadingCount = abnormalCount,
                LatestVitals = latestVitals,
                RecentRecords = recentRecords,
                TrendCharts = trendCharts,
                MyDoctors = doctors
            };
        }
    }
}