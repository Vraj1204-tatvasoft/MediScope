// File: MediScope.Business/Services/Interfaces/IDoctorService.cs

using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IDoctorService
    {
        /// <summary>Admin creates a doctor — sends welcome email with temp password</summary>
        Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorRequestDto request);

        /// <summary>Get any doctor by their doctor table PK — all roles</summary>
        Task<DoctorResponseDto> GetDoctorByIdAsync(Guid doctorId);

        /// <summary>Get logged-in doctor's own profile by userId from JWT</summary>
        Task<DoctorResponseDto> GetMyProfileAsync(Guid userId);

        /// <summary>Get all doctors — admin and patient use this</summary>
        Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync();

        /// <summary>Doctor updates their own profile — name, phone, hospital, experience, bio</summary>
        Task<DoctorResponseDto> UpdateMyProfileAsync(Guid userId, UpdateDoctorRequestDto request);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
    }
}