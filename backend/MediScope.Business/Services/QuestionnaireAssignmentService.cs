using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace MediScope.Business.Services
{
    public class QuestionnaireAssignmentService : IQuestionnaireAssignmentService
    {
        private readonly IQuestionnaireAssignmentRepository _repository;
        private readonly IPdfService _pdfService;
        private readonly ILogger<QuestionnaireAssignmentService> _logger;

        public QuestionnaireAssignmentService(
            IQuestionnaireAssignmentRepository repository,
            IPdfService pdfService,
            ILogger<QuestionnaireAssignmentService> logger)
        {
            _repository = repository;
            _pdfService = pdfService;
            _logger = logger;
        }

        public async Task<Guid> AssignQuestionnaireAsync(
            AssignQuestionnaireRequestDto request, Guid assignedBy)
        {
            if (request.QuestionnaireId == Guid.Empty)
                throw new ArgumentException("Questionnaire is required.");

            if (request.PatientId == Guid.Empty)
                throw new ArgumentException("Patient is required.");

            return await _repository.AssignQuestionnaireAsync(request, assignedBy);
        }

        public async Task UnassignQuestionnaireAsync(Guid assignmentId, Guid deletedBy)
        {
            if (assignmentId == Guid.Empty)
                throw new ArgumentException("Assignment ID is required.");

            await _repository.UnassignQuestionnaireAsync(assignmentId, deletedBy);
        }

        public async Task<PagedResult<PatientAssignmentResponseDto>> GetPatientAssignmentsAsync(
            Guid patientId, PatientAssignmentFilterDto filter)
        {
            return await _repository.GetPatientAssignmentsAsync(patientId, filter);
        }

        public async Task<PagedResult<DoctorAssignmentResponseDto>> GetDoctorAssignmentsAsync(
            Guid doctorId, DoctorAssignmentFilterDto filter)
        {
            return await _repository.GetDoctorAssignmentsAsync(doctorId, filter);
        }

        public async Task<DraftResponseDto> GetDraftResponsesAsync(
            Guid assignmentId, Guid patientId)
        {
            return await _repository.GetDraftResponsesAsync(assignmentId, patientId);
        }

        public async Task<SaveDraftResultDto> SaveDraftAsync(
            Guid assignmentId, Guid patientId, Guid userId, SaveDraftRequestDto request)
        {
            return await _repository.SaveDraftAsync(assignmentId, patientId, userId, request);
        }

        public async Task<SubmitResultDto> SubmitQuestionnaireAsync(
            Guid assignmentId, Guid patientId, Guid userId, SubmitQuestionnaireRequestDto request)
        {
            if (request.Responses is null || !request.Responses.Any())
                throw new ArgumentException("At least one response is required to submit.");

            var result = await _repository.SubmitQuestionnaireAsync(
                assignmentId, patientId, userId, request);
            try
            {
                var pdfPath = await _pdfService.GenerateSubmissionPdfAsync(
                    result.SubmissionId, patientId);

                await _repository.UpdatePdfPathAsync(result.SubmissionId, pdfPath);
                result.PdfPath = pdfPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "PDF generation failed for submission {SubmissionId}. " +
                    "Submission is saved but PDF path is not set.", result.SubmissionId);
            }

            return result;
        }

        public async Task<SubmissionDetailResponseDto> GetSubmissionDetailAsync(Guid submissionId)
        {
            var detail = await _repository.GetSubmissionDetailAsync(submissionId);

            if (detail is null)
                throw new KeyNotFoundException("Submission not found.");

            return detail;
        }
    }
}