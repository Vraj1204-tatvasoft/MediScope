using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;

namespace MediScope.Data.Repositories
{
    public class HealthMetricRepository
        : GenericRepository<HealthMetric>,
          IHealthMetricRepository
    {
        public HealthMetricRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<HealthMetric?> GetByIdWithDetailsAsync(Guid id)
            => await _dbSet
                .Include(h => h.Patient)
                    .ThenInclude(p => p.User)
                .Include(h => h.RecordedByUser)
                .Include(h => h.MetricDefinition)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

        public async Task<IEnumerable<HealthMetric>> GetAllWithMetricsAsync()
        {
            return await _dbSet
                .Include(h => h.Patient).ThenInclude(p => p.User)
                .Include(h => h.RecordedByUser)
                .Include(h => h.MetricDefinition)
                .Where(h => !h.IsDeleted)
                .OrderByDescending(h => h.RecordedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<HealthMetric>> GetPagedByPatientIdAsync(
             Guid patientId,
             PaginationParams pagination)
        {
            IQueryable<HealthMetric> baseQuery = _dbSet
                .Include(m => m.RecordedByUser)
                .Include(m => m.MetricDefinition)
                .Where(m => m.PatientId == patientId && !m.IsDeleted);

            // ── SEARCH ──────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var search = pagination.Search.ToLower().Trim();
                baseQuery = baseQuery.Where(m =>
                    m.RecordedByUser.FullName.ToLower().Contains(search) ||
                    m.RecordedAt.ToString().Contains(search));
            }

            // ── STATUS FILTER ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(pagination.Status) && pagination.Status != "ALL")
            {
                var statusUpper = pagination.Status.ToUpper();
                baseQuery = baseQuery.Where(m => m.Status == statusUpper);
            }

            // ── SOURCE FILTER ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(pagination.Source) && pagination.Source != "ALL")
            {
                var sourceUpper = pagination.Source.ToUpper();
                if (sourceUpper == "DOCTOR")
                    baseQuery = baseQuery.Where(m => m.RecordedByRole.ToUpper() != "PATIENT");
                else if (sourceUpper == "PATIENT")
                    baseQuery = baseQuery.Where(m => m.RecordedByRole.ToUpper() == "PATIENT");
            }

            // ── ISOLATE DISTINCT BATCHES FOR PAGINATION ─────────────
            var distinctBatchesQuery = baseQuery
                .Select(m => new
                {
                    m.SubmissionId,
                    m.RecordedAt,
                    m.Status,
                    FullName = m.RecordedByUser.FullName
                })
                .Distinct();

            var allMatchingStatuses = await distinctBatchesQuery.Select(b => b.Status).ToListAsync();

            int totalRecordsCount = allMatchingStatuses.Count;
            int normalCount = allMatchingStatuses.Count(s => s.ToUpper() == "NORMAL");
            int elevatedCount = allMatchingStatuses.Count(s => s.ToUpper() == "ELEVATED");
            int criticalCount = allMatchingStatuses.Count(s => s.ToUpper() == "CRITICAL");

            // ── SORTING ──────────────────────────────────────────────
            var isAsc = pagination.SortDir?.ToLower() == "asc";
            var sortBy = pagination.SortBy?.ToLower().Trim() ?? "date";

            distinctBatchesQuery = sortBy switch
            {
                "date" or "recordedat" => isAsc ? distinctBatchesQuery.OrderBy(b => b.RecordedAt) : distinctBatchesQuery.OrderByDescending(b => b.RecordedAt),
                "addedby" => isAsc ? distinctBatchesQuery.OrderBy(b => b.FullName) : distinctBatchesQuery.OrderByDescending(b => b.FullName),
                "status" => isAsc ? distinctBatchesQuery.OrderBy(b => b.Status) : distinctBatchesQuery.OrderByDescending(b => b.Status),
                _ => distinctBatchesQuery.OrderByDescending(b => b.RecordedAt)
            };

            // ── EXECUTE PAGINATION ON DISTINCT EVENTS ────────────────
            var pagedBatches = await distinctBatchesQuery
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var pagedSubmissionIds = pagedBatches.Select(b => b.SubmissionId).ToList();

            // ── FETCH ACTUAL METRICS FOR THOSE EVENTS ────────────────
            var items = await baseQuery
                .Where(m => pagedSubmissionIds.Contains(m.SubmissionId))
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();

            return new PagedResult<HealthMetric>
            {
                Items = items,
                TotalCount = totalRecordsCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                NormalCount = normalCount,
                ElevatedCount = elevatedCount,
                CriticalCount = criticalCount
            };
        }
    }
}