using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Common.Models.Entities;
namespace MediScope.Data.Repositories
{
    public interface IQuestionnaireAssignmentRepository
    {
        Task<Guid> AssignQuestionnaireAsync(AssignQuestionnaireRequestDto request, Guid assignedBy);
        Task UnassignQuestionnaireAsync(Guid assignmentId, Guid deletedBy);
        Task<PagedResult<PatientAssignmentResponseDto>> GetPatientAssignmentsAsync(Guid patientId, PatientAssignmentFilterDto filter);
        Task<PagedResult<DoctorAssignmentResponseDto>> GetDoctorAssignmentsAsync(Guid doctorId, DoctorAssignmentFilterDto filter);
        Task<DraftResponseDto> GetDraftResponsesAsync(Guid assignmentId, Guid patientId);
        Task<SaveDraftResultDto> SaveDraftAsync(Guid assignmentId, Guid patientId, Guid userId, SaveDraftRequestDto request);
        Task<SubmitResultDto> SubmitQuestionnaireAsync(Guid assignmentId, Guid patientId, Guid userId, SubmitQuestionnaireRequestDto request);
        Task UpdatePdfPathAsync(Guid submissionId, string pdfPath);
        Task<SubmissionDetailResponseDto?> GetSubmissionDetailAsync(Guid submissionId);
        Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid assignmentId);
        Task<List<SubmissionVersionResponseDto>> GetSubmissionVersionsAsync(Guid assignmentId);
    }
}