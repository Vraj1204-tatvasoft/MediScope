using MediScope.Common.Models.DTOs.RequestDTO;
using MediScope.Common.Models.DTOs.ResponseDTO;

namespace MediScope.Data.Repositories
{
    public interface IHospitalizationDashboardRepository
    {
        Task<HospitalizationDashboardResponseDto> GetDashboardAsync(HospitalizationDashboardFilterDto request);
    }
}