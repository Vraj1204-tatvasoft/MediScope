using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace MediScope.Business.Services.Interfaces
{
    public interface IQuestionnaireAssignmentService
    {
        Task<Guid> AssignQuestionnaireAsync(AssignQuestionnaireRequestDto request, Guid assignedBy);
        Task UnassignQuestionnaireAsync(Guid assignmentId, Guid deletedBy);
        Task<PagedResult<PatientAssignmentResponseDto>> GetPatientAssignmentsAsync(Guid patientId, PatientAssignmentFilterDto filter);
        Task<PagedResult<DoctorAssignmentResponseDto>> GetDoctorAssignmentsAsync(Guid doctorId, DoctorAssignmentFilterDto filter);
        Task<DraftResponseDto> GetDraftResponsesAsync(Guid assignmentId, Guid patientId);
        Task<SaveDraftResultDto> SaveDraftAsync(Guid assignmentId, Guid patientId, Guid userId, SaveDraftRequestDto request);
        Task<SubmitResultDto> SubmitQuestionnaireAsync(Guid assignmentId, Guid patientId, Guid userId, SubmitQuestionnaireRequestDto request);
        Task<SubmissionDetailResponseDto> GetSubmissionDetailAsync(Guid submissionId);
        Task<List<SubmissionVersionResponseDto>> GetSubmissionVersionsAsync(Guid assignmentId);
    }
}