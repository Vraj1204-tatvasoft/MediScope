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
        Task DischargePatientAsync(Guid admissionId, string dischargeNotes, DateTime dischargeDate);
        Task<PagedResult<AdmissionSummaryDto>> GetAdmissionsPagedAsync(PaginationParams request);
        Task<AdmissionDetailsDto?> GetAdmissionByIdAsync(Guid admissionId);
        Task UpdateAdmissionAsync(Guid admissionId, UpdateAdmissionRequestDto request);
        Task<List<RoomPatientDto>> GetActivePatientsByRoomAsync(Guid roomId);
        Task CheckInPatientAsync(Guid admissionId);
        Task CancelAdmissionAsync(Guid admissionId);
        Task<AvailableBedResponseDto?> GetFirstAvailableBedAsync(Guid roomId, DateTime start, DateTime end);
    }
}