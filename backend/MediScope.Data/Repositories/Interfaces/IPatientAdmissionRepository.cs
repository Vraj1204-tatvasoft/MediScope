using MediScope.Common.Models.Entities;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
namespace MediScope.Data.Repositories
{
    public interface IPatientAdmissionRepository
    {
        Task AdmitPatientAsync(AdmitPatientRequestDto request);
        Task TransferPatientBedAsync(Guid admissionId, TransferBedRequestDto request);
        Task DischargePatientAsync(Guid admissionId, string dischargeNotes);
        Task<PagedResult<AdmissionSummaryDto>> GetAdmissionsPagedAsync(PaginationParams request);
        Task<AdmissionDetailsDto?> GetAdmissionByIdAsync(Guid admissionId);
        Task UpdateAdmissionAsync(Guid admissionId, UpdateAdmissionRequestDto request);
        Task<List<RoomPatientDto>> GetActivePatientsByRoomAsync(Guid roomId);
    }
}