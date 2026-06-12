using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;

namespace MediScope.Business.Services.Interfaces
{
    public interface IHealthMetricService
    {
        // CREATE HEALTH RECORD

        Task<HealthMetricSubmissionResponseDto>
            AddMetricAsync(
                AddHealthMetricRequestDto request,
                Guid callerUserId,
                string callerRole);

        // GET SINGLE SUBMISSION

        Task<HealthMetricSubmissionResponseDto>
            GetByIdAsync(
                Guid id,
                Guid callerUserId,
                string callerRole);


        // GET LOGGED-IN PATIENT HISTORY

        Task<PagedResult<HealthMetricSubmissionResponseDto>>
            GetPagedForLoggedInPatientAsync(
                Guid userId,
                PaginationParams pagination);

        Task<PagedResult<HealthMetricSubmissionResponseDto>>
           GetAllByPatientAsync(
               Guid patientId,
               PaginationParams pagination,
               Guid callerUserId,
               string callerRole);
        Task DeleteSubmissionAsync(Guid id, Guid callerUserId, string callerRole);
    }
}