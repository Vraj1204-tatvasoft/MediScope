using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class HealthMetricService : IHealthMetricService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public HealthMetricService(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        //  ADD HEALTH RECORD 
        public async Task<HealthMetricSubmissionResponseDto> AddMetricAsync(AddHealthMetricRequestDto request, Guid callerUserId, string callerRole)
        {
            Guid targetPatientId;

            // RESOLVE PATIENT CONTEXT
            if (callerRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patientProfile = await _uow.Patients.GetFirstOrDefaultAsync(p => p.UserId == callerUserId && !p.IsDeleted)
                    ?? throw new UnauthorizedAccessException("Your patient profile was not found.");
                targetPatientId = patientProfile.Id;
            }
            else if (callerRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.PatientId.HasValue) throw new ArgumentException("Target Patient ID must be specified.");
                targetPatientId = request.PatientId.Value;
            }
            else
            {
                throw new UnauthorizedAccessException("Your assigned role permissions are not authorized to log health metrics.");
            }

            await ValidateCallerAccessAsync(callerUserId, callerRole, targetPatientId);

            var targetPatient = await _uow.Patients.GetByIdAsync(targetPatientId)
                ?? throw new Exception("Patient not found.");

            // PREPARE THE BATCH ID (The Grouping Tag)
            var sharedSubmissionId = Guid.NewGuid();
            bool elevated = false;
            bool critical = false;

            var metricsToInsert = new List<HealthMetric>();

            // BUILD INDIVIDUAL METRICS AND DETERMINE STATUS
            foreach (var metricRequest in request.Metrics)
            {
                var metricDef = await _uow.MetricDefinitions.GetFirstOrDefaultAsync(m => m.MetricType == metricRequest.MetricType && !m.IsDeleted)
                    ?? throw new KeyNotFoundException($"Metric type '{metricRequest.MetricType}' not found.");

                // HIGH CHECK
                if (metricDef.NormalMax.HasValue && metricRequest.Value > metricDef.NormalMax)
                {
                    elevated = true;
                    if (metricRequest.Value > metricDef.NormalMax * 1.2m) critical = true;
                }

                // LOW CHECK
                if (metricDef.NormalMin.HasValue && metricRequest.Value < metricDef.NormalMin)
                {
                    elevated = true;
                    if (metricRequest.Value < metricDef.NormalMin * 0.8m) critical = true;
                }

                metricsToInsert.Add(new HealthMetric
                {
                    SubmissionId = sharedSubmissionId,
                    MetricType = metricRequest.MetricType,
                    Value = metricRequest.Value,
                    Unit = string.IsNullOrWhiteSpace(metricRequest.Unit) ? metricDef.DefaultUnit : metricRequest.Unit,

                    // Flattened Metadata
                    PatientId = targetPatientId,
                    RecordedByUserId = callerUserId,
                    RecordedByRole = callerRole,
                    RecordedAt = request.RecordedAt,
                    Notes = request.Notes,

                    CreatedBy = callerUserId,
                    UpdatedBy = callerUserId
                });
            }

            // 4. APPLY OVERALL STATUS TO ALL ROWS IN THE BATCH
            string finalStatus = critical ? "CRITICAL" : elevated ? "ELEVATED" : "NORMAL";
            foreach (var m in metricsToInsert)
            {
                m.Status = finalStatus;
                await _uow.HealthMetrics.AddAsync(m);
            }

            await _uow.SaveChangesAsync();

            // 5. ALERT NOTIFICATIONS
            if (critical)
                await _notificationService.CreateAsync(targetPatient.UserId, "alert", "Critical health readings detected. Please consult your doctor immediately.");
            else if (elevated)
                await _notificationService.CreateAsync(targetPatient.UserId, "alert", "Some health readings are outside the normal range.");

            if (callerRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctorUser = await _uow.Users.GetByIdAsync(callerUserId);
                await _notificationService.CreateAsync(targetPatient.UserId, "info", $"Dr. {doctorUser?.FullName ?? "Doctor"} added a health record on your behalf.");
            }

            // Return the newly mapped object directly
            return Map(metricsToInsert);
        }

        //  GET PAGED HISTORY 
        public async Task<PagedResult<HealthMetricSubmissionResponseDto>> GetPagedForLoggedInPatientAsync(Guid userId, PaginationParams pagination)
        {
            var patient = await _uow.Patients.GetFirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted)
                ?? throw new UnauthorizedAccessException("Patient not found.");

            return await GetPagedHistoryInternalAsync(patient.Id, pagination);
        }

        public async Task<PagedResult<HealthMetricSubmissionResponseDto>> GetAllByPatientAsync(Guid patientId, PaginationParams pagination, Guid callerUserId, string callerRole)
        {
            await ValidateCallerAccessAsync(callerUserId, callerRole, patientId);
            return await GetPagedHistoryInternalAsync(patientId, pagination);
        }

        private async Task<PagedResult<HealthMetricSubmissionResponseDto>> GetPagedHistoryInternalAsync(Guid patientId, PaginationParams pagination)
        {
            // Call our newly updated repository method that handles the distinct batching
            var pagedResult = await _uow.HealthMetrics.GetPagedByPatientIdAsync(patientId, pagination);

            // Group the flat SQL rows back into the hierarchical JSON structure the UI expects
            var groupedSubmissions = pagedResult.Items
                .GroupBy(m => m.SubmissionId)
                .Select(group => Map(group.ToList()))
                .ToList();

            return new PagedResult<HealthMetricSubmissionResponseDto>
            {
                Items = groupedSubmissions,
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                SummaryStats = new SubmissionSummaryStatsDto
                {
                    TotalRecords = pagedResult.TotalCount,
                    Normal = pagedResult.NormalCount,
                    Elevated = pagedResult.ElevatedCount,
                    Critical = pagedResult.CriticalCount
                }
            };
        }

        //  GET SINGLE SUBMISSION 
        public async Task<HealthMetricSubmissionResponseDto> GetByIdAsync(Guid id, Guid callerUserId, string callerRole)
        {
            // Because there's no GetById for a batch, we fetch all metrics sharing this SubmissionId
            var metrics = await _uow.HealthMetrics.FindAsync(m => m.SubmissionId == id && !m.IsDeleted);

            if (metrics == null || !metrics.Any())
                throw new KeyNotFoundException("Submission not found.");

            await ValidateCallerAccessAsync(callerUserId, callerRole, metrics.First().PatientId);

            // You might need to manually include definitions/users depending on your FindAsync implementation, 
            // but the mapping function gracefully handles nulls via `?.`
            return Map(metrics.ToList());
        }

        //  SOFT DELETE 
        public async Task DeleteSubmissionAsync(Guid id, Guid callerUserId, string callerRole)
        {
            var metrics = await _uow.HealthMetrics.FindAsync(m => m.SubmissionId == id && !m.IsDeleted);

            if (metrics == null || !metrics.Any())
                throw new KeyNotFoundException("The requested health submission record could not be found.");

            var patientId = metrics.First().PatientId;

            if (callerRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patientProfile = await _uow.Patients.GetFirstOrDefaultAsync(p => p.UserId == callerUserId && !p.IsDeleted)
                    ?? throw new UnauthorizedAccessException("Patient profile mismatch.");

                if (patientId != patientProfile.Id)
                    throw new UnauthorizedAccessException("Access Denied: You cannot delete medical charts belonging to other patient files.");
            }
            else
            {
                await ValidateCallerAccessAsync(callerUserId, callerRole, patientId);
            }

            // Loop and soft-delete all metrics in this batch
            var now = DateTime.UtcNow;
            foreach (var metric in metrics)
            {
                metric.IsDeleted = true;
                metric.DeletedAt = now;
                metric.DeletedBy = callerUserId;
                metric.UpdatedBy = callerUserId;
                metric.UpdatedAt = now;
                _uow.HealthMetrics.Update(metric);
            }

            await _uow.SaveChangesAsync();
        }

        //  ACCESS VALIDATION
        private async Task ValidateCallerAccessAsync(Guid callerUserId, string callerRole, Guid patientId)
        {
            switch (callerRole)
            {
                case "Patient":
                    var patient = await _uow.Patients.GetFirstOrDefaultAsync(p => p.UserId == callerUserId && !p.IsDeleted)
                        ?? throw new UnauthorizedAccessException("Patient profile not found.");
                    if (patient.Id != patientId) throw new UnauthorizedAccessException("Unauthorized access.");
                    break;

                case "Doctor":
                    var doctor = await _uow.Doctors.GetByUserIdAsync(callerUserId)
                        ?? throw new UnauthorizedAccessException("Doctor profile not found.");
                    var isAssigned = await _uow.DoctorPatients.AnyAsync(dp => dp.DoctorId == doctor.Id
                        && dp.PatientId == patientId
                        && (dp.Status == "active" || dp.Status == "pending_doctor")
                        && !dp.IsDeleted);
                    if (!isAssigned) throw new UnauthorizedAccessException("Doctor not assigned.");
                    break;

                case "Admin":
                    break;

                default:
                    throw new UnauthorizedAccessException("Invalid role.");
            }
        }

        // ── MAPPING ────────────────────────────────────────────────────
        // Rebuilds the DTO from a grouped list of identical event rows
        private static HealthMetricSubmissionResponseDto Map(IEnumerable<HealthMetric> metricGroup)
        {
            var first = metricGroup.First();

            return new HealthMetricSubmissionResponseDto
            {
                SubmissionId = first.SubmissionId,
                PatientId = first.PatientId,
                RecordedByUserId = first.RecordedByUserId,
                RecordedByRole = first.RecordedByRole,
                RecordedByName = first.RecordedByUser?.FullName ?? string.Empty,
                RecordedAt = first.RecordedAt,
                Notes = first.Notes,
                Status = first.Status,
                CreatedAt = first.CreatedAt,
                Metrics = metricGroup.Select(m => new HealthMetricItemResponseDto
                {
                    Id = m.Id,
                    MetricType = m.MetricType,
                    DisplayName = m.MetricDefinition?.DisplayName ?? m.MetricType,
                    Value = m.Value,
                    Unit = m.Unit,
                    NormalMin = m.MetricDefinition?.NormalMin,
                    NormalMax = m.MetricDefinition?.NormalMax
                }).ToList()
            };
        }
    }
}