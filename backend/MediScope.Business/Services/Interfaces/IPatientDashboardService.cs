using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IPatientDashboardService
    {
        Task<PatientDashboardResponseDto> GetDashboardAsync(Guid userId);
    }
}