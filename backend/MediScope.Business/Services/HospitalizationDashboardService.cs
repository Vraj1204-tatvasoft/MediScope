using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.RequestDTO;
using MediScope.Common.Models.DTOs.ResponseDTO;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class HospitalizationDashboardService : IHospitalizationDashboardService
    {
        private readonly IHospitalizationDashboardRepository _repository;
        public HospitalizationDashboardService(IHospitalizationDashboardRepository repository)
        {
            _repository = repository;
        }
        public async Task<HospitalizationDashboardResponseDto> GetDashboardAsync(HospitalizationDashboardFilterDto request)
        {
            return await _repository.GetDashboardAsync(request);
        }
    }
}