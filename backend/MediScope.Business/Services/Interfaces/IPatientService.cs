// File: MediScope.Business/Services/Interfaces/IPatientService.cs

using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;

namespace MediScope.Business.Services.Interfaces
{
    public interface IPatientService
        : IGenericService<Patient, PatientProfileResponseDto, UpdateProfileRequestDto, UpdateProfileRequestDto>
    {
        /// <summary>Get profile by userId (from JWT) — not patient table PK</summary>
        Task<PatientProfileResponseDto> GetMyProfileAsync(Guid userId);
        /// <summary>Update profile by userId</summary>
        Task<PatientProfileResponseDto> UpdateMyProfileAsync(Guid userId, UpdateProfileRequestDto request);
        /// <summary>Change password after verifying current password</summary>
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
        Task<AdminPatientOverviewDto>
        GetAdminPatientsAsync(AdminPatientFilterDto filter, PaginationParams pagination);
    }
}