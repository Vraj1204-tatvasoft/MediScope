using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IDoctorDashboardRepository
    {
        Task<List<VitalTrendFlatResult>> CallVitalTrendsFunctionAsync(Guid doctorId, string metricType, string patientId, DateTime start, DateTime end);
    }
}