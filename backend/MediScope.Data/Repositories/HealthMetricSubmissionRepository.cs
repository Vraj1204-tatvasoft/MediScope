// File: MediScope.Data/Repositories/HealthMetricSubmissionRepository.cs

/*using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;

namespace MediScope.Data.Repositories
{
    public class HealthMetricSubmissionRepository
        : GenericRepository<HealthMetricSubmission>,
          IHealthMetricSubmissionRepository
    {
        public HealthMetricSubmissionRepository(AppDbContext context)
            : base(context) { }

        public async Task<HealthMetricSubmission?> GetByIdWithDetailsAsync(Guid id)
            => await _dbSet
                .Include(s => s.Patient).ThenInclude(p => p.User)
                .Include(s => s.RecordedByUser)
                .Include(s => s.Metrics).ThenInclude(m => m.MetricDefinition)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        public async Task<PagedResult<HealthMetricSubmission>> GetPagedByPatientIdAsync(
             Guid patientId,
             PaginationParams pagination)
        {
            IQueryable<HealthMetricSubmission> query = _dbSet
                .Include(s => s.RecordedByUser)
                .Include(s => s.Metrics).ThenInclude(m => m.MetricDefinition)
                .Where(s => s.PatientId == patientId && !s.IsDeleted);

            // ── SEARCH ──────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var search = pagination.Search.ToLower().Trim();
                query = query.Where(s =>
                    s.RecordedByUser.FullName.ToLower().Contains(search) ||
                    s.RecordedAt.ToString().Contains(search));
            }

            // ── STATUS FILTER ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(pagination.Status) && pagination.Status != "ALL")
            {
                var statusUpper = pagination.Status.ToUpper();
                query = query.Where(s => s.Status == statusUpper);
            }

            // ── SOURCE FILTER ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(pagination.Source) && pagination.Source != "ALL")
            {
                var sourceUpper = pagination.Source.ToUpper();
                if (sourceUpper == "DOCTOR")
                    query = query.Where(s => s.RecordedByRole.ToUpper() != "PATIENT");
                else if (sourceUpper == "PATIENT")
                    query = query.Where(s => s.RecordedByRole.ToUpper() == "PATIENT");
            }

            // ── FIXED: FETCH TOTALS AS PLAIN IN-MEMORY STRINGS ──
            var allMatchingStatuses = await query.Select(s => s.Status).ToListAsync();

            int totalRecordsCount = allMatchingStatuses.Count;
            int normalCount = allMatchingStatuses.Count(s => s.ToUpper() == "NORMAL");
            int elevatedCount = allMatchingStatuses.Count(s => s.ToUpper() == "ELEVATED");
            int criticalCount = allMatchingStatuses.Count(s => s.ToUpper() == "CRITICAL");

            // ── SORTING ──────────────────────────────────────────────
            var isAsc = pagination.SortDir?.ToLower() == "asc";
            var sortBy = pagination.SortBy?.ToLower().Trim() ?? "date";

            query = sortBy switch
            {
                "date" or "recordedat" => isAsc ? query.OrderBy(s => s.RecordedAt) : query.OrderByDescending(s => s.RecordedAt),
                "addedby" => isAsc ? query.OrderBy(s => s.RecordedByUser.FullName) : query.OrderByDescending(s => s.RecordedByUser.FullName),
                "status" => isAsc ? query.OrderBy(s => s.Status) : query.OrderByDescending(s => s.Status),
            };

            // ── PAGINATION SLICING ──────────────────────────────────
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            // Store counts temporarily inside base class properties for our service layer to map later
            return new PagedResult<HealthMetricSubmission>
            {
                Items = items,
                TotalCount = totalRecordsCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,

                // Pass properties safely using custom layout tags
                NormalCount = normalCount,
                ElevatedCount = elevatedCount,
                CriticalCount = criticalCount
            };
        }
        public async Task SoftDeleteWithMetricsAsync(Guid submissionId, Guid deletedByUserId)
        {
            var submission = await _dbSet
                .Include(s => s.Metrics)
                .FirstOrDefaultAsync(s => s.Id == submissionId && !s.IsDeleted);

            if (submission != null)
            {
                var now = DateTime.UtcNow;

                submission.IsDeleted = true;
                submission.DeletedAt = now;
                submission.DeletedBy = deletedByUserId;
                submission.UpdatedBy = deletedByUserId;
                submission.UpdatedAt = now;

                foreach (var metric in submission.Metrics)
                {
                    metric.IsDeleted = true;
                    metric.DeletedAt = now;
                    metric.DeletedBy = deletedByUserId;
                    metric.UpdatedBy = deletedByUserId;
                    metric.UpdatedAt = now;
                }

                _dbSet.Update(submission);
            }
        }
        public async Task<IEnumerable<HealthMetricSubmission>> GetAllWithMetricsAsync()
        => await _dbSet
            .Include(s => s.Patient).ThenInclude(p => p.User)
            .Include(s => s.RecordedByUser)
            .Include(s => s.Metrics).ThenInclude(m => m.MetricDefinition)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.RecordedAt)
            .ToListAsync();
    }
}*/