using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IPatientDashboardRepository
    {
        Task<Patient?> GetDashboardDataAsync(Guid userId);
    }
}