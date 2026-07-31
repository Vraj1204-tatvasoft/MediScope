using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;
using MediScope.Common.Models.Enums;
namespace MediScope.Business.Services
{
    public class QuestionnaireAssignmentService : IQuestionnaireAssignmentService
    {
        private readonly IQuestionnaireAssignmentRepository _repository;
        private readonly IPdfService _pdfService;
        private readonly ILogger<QuestionnaireAssignmentService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IPatientRepository _patientRepository;

        public QuestionnaireAssignmentService(
            IQuestionnaireAssignmentRepository repository,
            IPdfService pdfService,
            ILogger<QuestionnaireAssignmentService> logger,
            INotificationService notificationService,
            IPatientRepository patientRepository)
        {
            _repository = repository;
            _pdfService = pdfService;
            _logger = logger;
            _notificationService = notificationService;
            _patientRepository = patientRepository;
        }

        public async Task<Guid> AssignQuestionnaireAsync(
            AssignQuestionnaireRequestDto request, Guid assignedBy)
        {
            if (request.QuestionnaireId == Guid.Empty)
                throw new ArgumentException("Questionnaire is required.");

            if (request.PatientId == Guid.Empty)
                throw new ArgumentException("Patient is required.");

            var assignmentId = await _repository.AssignQuestionnaireAsync(request, assignedBy);
            var patient = await _patientRepository.GetPatientByIdAsync(request.PatientId);
            // 1. Notify the Patient
            try
            {
                await _notificationService.CreateAsync(
                    userId: patient.UserId,
                    type: NotificationType.Info,
                    message: "Your doctor has assigned a new questionnaire for you to complete.",
                    referenceType: "QuestionnaireAssignment",
                    referenceId: assignmentId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send assignment notification to patient {PatientId}", request.PatientId);
            }

            return assignmentId;
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
            try
            {
                var assignment = await _repository.GetAssignmentByIdAsync(assignmentId);

                if (assignment != null && assignment.AssignedBy != Guid.Empty)
                {
                    await _notificationService.CreateAsync(
                        userId: assignment.AssignedBy,
                        type: NotificationType.Info,
                        message: "A patient has submitted their assigned questionnaire responses.",
                        referenceType: "QuestionnaireSubmission",
                        referenceId: patientId
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send submission notification to doctor");
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