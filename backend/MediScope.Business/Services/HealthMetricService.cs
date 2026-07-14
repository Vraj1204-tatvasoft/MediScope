using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Enums;

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
            bool isUpdate = request.SubmissionId.HasValue && request.SubmissionId.Value != Guid.Empty;
            Guid targetPatientId;
            List<HealthMetric> existingMetrics = new List<HealthMetric>();

            // 1. RESOLVE CONTEXT AND VALIDATE SECURITY
            if (isUpdate)
            {
                existingMetrics = (await _uow.HealthMetrics.FindAsync(m => m.SubmissionId == request.SubmissionId.Value && !m.IsDeleted)).ToList();

                if (!existingMetrics.Any())
                    throw new KeyNotFoundException("The requested health submission could not be found.");

                if (existingMetrics.First().RecordedByUserId != callerUserId)
                    throw new UnauthorizedAccessException("Access Denied: You can only edit health records that you personally recorded.");

                targetPatientId = existingMetrics.First().PatientId;
            }
            else
            {
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
            }

            // (Your existing appointment validation code remains here...)
            if (request.AppointmentId.HasValue)
            {
                var appointment = await _uow.Appointments.GetByIdAsync(request.AppointmentId.Value)
                    ?? throw new KeyNotFoundException("The specified appointment does not exist.");

                if (appointment.PatientId != targetPatientId)
                {
                    throw new UnauthorizedAccessException("The appointment provided does not belong to the target patient.");
                }
            }

            // 2. PREPARE UPSERT VARIABLES
            var sharedSubmissionId = isUpdate ? request.SubmissionId.Value : Guid.NewGuid();
            bool elevated = false;
            bool critical = false;
            var metricsToProcess = new List<HealthMetric>();
            var now = DateTime.UtcNow;

            // 3. PROCESS EACH METRIC IN THE REQUEST
            foreach (var metricRequest in request.Metrics)
            {
                var metricDef = await _uow.MetricDefinitions.GetFirstOrDefaultAsync(m => m.MetricType == metricRequest.MetricType && !m.IsDeleted)
                    ?? throw new KeyNotFoundException($"Metric type '{metricRequest.MetricType}' not found.");

                // HIGH/LOW STATUS CHECKS
                if (metricDef.NormalMax.HasValue && metricRequest.Value > metricDef.NormalMax)
                {
                    elevated = true;
                    if (metricRequest.Value > metricDef.NormalMax * 1.2m) critical = true;
                }
                if (metricDef.NormalMin.HasValue && metricRequest.Value < metricDef.NormalMin)
                {
                    elevated = true;
                    if (metricRequest.Value < metricDef.NormalMin * 0.8m) critical = true;
                }

                HealthMetric metricEntity;

                if (isUpdate)
                {
                    // Check if this specific metric type (e.g., Blood Pressure) already exists in this submission
                    metricEntity = existingMetrics.FirstOrDefault(m => m.MetricType == metricRequest.MetricType);

                    if (metricEntity != null)
                    {
                        // UPDATE existing row
                        metricEntity.Value = metricRequest.Value;
                        metricEntity.Unit = string.IsNullOrWhiteSpace(metricRequest.Unit) ? metricDef.DefaultUnit : metricRequest.Unit;
                        metricEntity.RecordedAt = request.RecordedAt;
                        metricEntity.Notes = request.Notes;
                        metricEntity.UpdatedBy = callerUserId;
                        metricEntity.UpdatedAt = now;

                        _uow.HealthMetrics.Update(metricEntity);
                    }
                    else
                    {
                        // INSERT new row (User added a new metric type during the edit)
                        metricEntity = CreateNewMetricEntity(metricRequest, metricDef, sharedSubmissionId, request, targetPatientId, callerUserId, callerRole, now);
                        await _uow.HealthMetrics.AddAsync(metricEntity);
                    }
                }
                else
                {
                    // INSERT new row (Standard Create Flow)
                    metricEntity = CreateNewMetricEntity(metricRequest, metricDef, sharedSubmissionId, request, targetPatientId, callerUserId, callerRole, now);
                    await _uow.HealthMetrics.AddAsync(metricEntity);
                }

                metricsToProcess.Add(metricEntity);
            }

            // 4. CLEANUP: REMOVE DELETED METRICS (If user removed a metric during an edit)
            if (isUpdate)
            {
                var requestedTypes = request.Metrics.Select(m => m.MetricType).ToList();
                var removedMetrics = existingMetrics.Where(m => !requestedTypes.Contains(m.MetricType));

                foreach (var removed in removedMetrics)
                {
                    removed.IsDeleted = true;
                    removed.DeletedAt = now;
                    removed.DeletedBy = callerUserId;
                    _uow.HealthMetrics.Update(removed);
                }
            }

            // 5. APPLY OVERALL STATUS TO ALL ACTIVE ROWS IN BATCH
            Severity finalStatus = critical ? Severity.Critical : elevated ? Severity.Elevated : Severity.Normal;
            foreach (var m in metricsToProcess)
            {
                m.Status = finalStatus;
            }

            await _uow.SaveChangesAsync();

            // 6. NOTIFICATIONS
            // Only fire on new submissions, not edits, to avoid spamming.
            if (!isUpdate)
            {
                var patientProfile = await _uow.Patients.GetByIdAsync(targetPatientId);
                var patientUserId = patientProfile?.UserId;

                if (patientUserId.HasValue)
                {
                    if (critical)
                    {
                        await _notificationService.CreateAsync(
                            patientUserId.Value,
                            NotificationType.Alert,
                            "Critical health readings detected. Please consult your doctor immediately.",
                            referenceType: "health",
                            referenceId: sharedSubmissionId
                        );
                    }
                    else if (elevated)
                    {
                        await _notificationService.CreateAsync(
                            patientUserId.Value,
                            NotificationType.Alert,
                            "Some health readings are outside the normal range.",
                            referenceType: "health",
                            referenceId: sharedSubmissionId
                        );
                    }
                }

                // If a doctor logged the metrics, also notify them so it appears in their feed.
                if (callerRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
                {
                    var doctorUser = await _uow.Users.GetByIdAsync(callerUserId);
                    await _notificationService.CreateAsync(patientUserId.Value, NotificationType.Info, $"Dr. {doctorUser?.FullName ?? "Doctor"} added a health record on your behalf.", referenceType: "health");
                }

            }

            return Map(metricsToProcess);
        }

        //  GET PAGED HISTORY 
        public async Task<PagedResult<HealthMetricSubmissionResponseDto>> GetPagedForLoggedInPatientAsync(Guid userId, PaginationParams pagination)
        {
            var patient = await _uow.Patients.GetFirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted)
                ?? throw new UnauthorizedAccessException("Patient profile not found.");

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

            return Map(metrics.ToList());
        }

        //  SOFT DELETE 
        public async Task DeleteSubmissionAsync(Guid id, Guid callerUserId, string callerRole)
        {
            var metrics = await _uow.HealthMetrics.FindAsync(m => m.SubmissionId == id && !m.IsDeleted);

            if (metrics == null || !metrics.Any())
                throw new KeyNotFoundException("The requested health submission record could not be found.");

            var firstMetric = metrics.First();

            if (firstMetric.RecordedByUserId != callerUserId)
            {
                throw new UnauthorizedAccessException("Access Denied: You can only delete health records that you personally recorded.");
            }

            var patientId = firstMetric.PatientId;

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
                        && (dp.Status == ConnectionStatus.Active || dp.Status == ConnectionStatus.PendingDoctor)
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
                AppointmentId = first.AppointmentId,
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

        private HealthMetric CreateNewMetricEntity(AddMetricValueRequestDto req, MetricDefinition def, Guid subId, AddHealthMetricRequestDto fullReq, Guid patientId, Guid userId, string role, DateTime time)
        {
            return new HealthMetric
            {
                SubmissionId = subId,
                AppointmentId = fullReq.AppointmentId,
                MetricType = req.MetricType,
                Value = req.Value,
                Unit = string.IsNullOrWhiteSpace(req.Unit) ? def.DefaultUnit : req.Unit,
                PatientId = patientId,
                RecordedByUserId = userId,
                RecordedByRole = role,
                RecordedAt = fullReq.RecordedAt,
                Notes = fullReq.Notes,
                CreatedBy = userId,
                UpdatedBy = userId,
                UpdatedAt = time
            };
        }
    }
}