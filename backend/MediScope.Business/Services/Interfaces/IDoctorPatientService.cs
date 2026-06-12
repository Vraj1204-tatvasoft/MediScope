using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IDoctorPatientService
    {
        Task<PatientDoctorResponseDto> SendRequestAsync(Guid patientUserId, SendDoctorRequestDto request);
        Task RevokeAccessAsync(Guid patientUserId, RevokeAccessDto request);
        Task<IEnumerable<PatientDoctorResponseDto>> GetMyDoctorsAsync(Guid patientUserId);
        Task<DoctorPatientResponseDto> RespondToRequestAsync(Guid doctorUserId, RespondToRequestDto request);
        Task<IEnumerable<DoctorPatientResponseDto>> GetPendingRequestsAsync(Guid doctorUserId);
        Task<IEnumerable<DoctorPatientResponseDto>> GetMyPatientsAsync(Guid doctorUserId);
        Task<AdminDoctorPatientOverviewDto> GetAdminOverviewAsync(AdminDoctorPatientFilterDto filter);
        Task<PatientDoctorResponseDto> ApproveRequestAsync(Guid adminUserId, AdminApproveRequestDto request);
        Task RejectRequestAsync(Guid adminUserId, AdminRejectRequestDto request);
        Task<IEnumerable<AdminConnectionRequestDto>> GetPendingAdminRequestsAsync();
        Task<IEnumerable<AdminConnectionRequestDto>> GetAllRequestsForAdminAsync(AdminDoctorPatientFilterDto filter);
    }
}