// File: MediScope.Business/Services/Interfaces/IDoctorDashboardService.cs

using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IDoctorDashboardService
    {
        /// <summary>
        /// Returns all data for the doctor dashboard in one call.
        /// All data is scoped to only the patients assigned to this doctor.
        /// </summary>
        Task<DoctorDashboardResponseDto> GetDashboardAsync(Guid doctorUserId);
        Task<List<VitalTrendResponseDto>> GetVitalTrendsAsync(Guid doctorUserId, string metricType, string patientId, string duration, DateTime? fromDate, DateTime? toDate);
    }
}