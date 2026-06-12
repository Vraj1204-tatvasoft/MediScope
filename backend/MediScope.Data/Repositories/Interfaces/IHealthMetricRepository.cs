using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;
namespace MediScope.Data.Repositories
{
    public interface IHealthMetricRepository : IGenericRepository<HealthMetric>
    {
        Task<HealthMetric?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<HealthMetric>> GetAllWithMetricsAsync();
        Task<PagedResult<HealthMetric>> GetPagedByPatientIdAsync(Guid patientId, PaginationParams pagination);
    }
}