using MediScope.Common.Models.DTOs.RequestDTO;
using MediScope.Common.Models.DTOs.ResponseDTO;

namespace MediScope.Business.Services.Interfaces
{
    public interface IHospitalizationDashboardService
    {
        Task<HospitalizationDashboardResponseDto> GetDashboardAsync(HospitalizationDashboardFilterDto request);
    }
}