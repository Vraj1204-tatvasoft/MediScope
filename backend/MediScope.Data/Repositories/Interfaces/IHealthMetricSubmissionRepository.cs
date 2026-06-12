/*using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;

namespace MediScope.Data.Repositories
{
    public interface IHealthMetricSubmissionRepository
        : IGenericRepository<HealthMetricSubmission>
    {
        Task<HealthMetricSubmission?> GetByIdWithDetailsAsync(Guid id);

        Task<PagedResult<HealthMetricSubmission>> GetPagedByPatientIdAsync(Guid patientId, PaginationParams pagination);
        Task SoftDeleteWithMetricsAsync(Guid submissionId, Guid deletedByUserId);
        Task<IEnumerable<HealthMetricSubmission>> GetAllWithMetricsAsync();
    }
}*/