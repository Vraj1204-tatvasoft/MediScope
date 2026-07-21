using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services.Interfaces
{
    public interface IPatientAdmissionService
    {
        Task<bool> AdmitPatientAsync(AdmitPatientRequestDto request);
        Task<bool> TransferPatientBedAsync(Guid admissionId, TransferBedRequestDto request);
        Task<bool> DischargePatientAsync(Guid admissionId, DischargePatientRequestDto request);
        Task<PagedResult<AdmissionSummaryDto>> GetAdmissionsPagedAsync(PaginationParams request);
        Task<AdmissionDetailsDto> GetAdmissionByIdAsync(Guid admissionId);
        Task<bool> UpdateAdmissionAsync(Guid admissionId, UpdateAdmissionRequestDto request);
        Task<List<RoomPatientDto>> GetActivePatientsByRoomAsync(Guid roomId);
    }
}