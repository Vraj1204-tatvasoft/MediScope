using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Pagination;

namespace MediScope.API.Controllers.Features
{
    [ApiController]
    [Route("api")]
    public class QuestionnaireAssignmentController : BaseController
    {
        private readonly IQuestionnaireAssignmentService _service;
        private readonly IQuestionnaireService _questionnaireService;

        public QuestionnaireAssignmentController(
            IQuestionnaireAssignmentService service,
            IQuestionnaireService questionnaireService)
        {
            _service = service;
            _questionnaireService = questionnaireService;
        }

        // ASSIGNMENT — Doctor side

        /// <summary>Doctor assigns a questionnaire to a patient.</summary>
        [HttpPost("questionnaire-assignments")]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> AssignQuestionnaire(
            [FromBody] AssignQuestionnaireRequestDto request)
        {
            var id = await _service.AssignQuestionnaireAsync(request, CurrentUserId);
            return Created(new { AssignmentId = id }, "Questionnaire assigned successfully.");
        }

        /// <summary>Doctor removes an assignment (only if not yet submitted).</summary>
        [HttpDelete("questionnaire-assignments/{assignmentId:guid}")]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> UnassignQuestionnaire(Guid assignmentId)
        {
            await _service.UnassignQuestionnaireAsync(assignmentId, CurrentUserId);
            return NoContent("Assignment removed successfully.");
        }

        /// <summary>
        /// Doctor views all assignments they made.
        /// Optionally filtered by patientId.
        /// </summary>
        [HttpGet("doctors/{doctorId:guid}/questionnaire-assignments")]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> GetDoctorAssignments(
            Guid doctorId, [FromQuery] DoctorAssignmentFilterDto filter)
        {
            var result = await _service.GetDoctorAssignmentsAsync(doctorId, filter);
            return Success(result);
        }

        /// <summary>
        /// Returns the full questionnaire structure for dynamic rendering,
        /// pre-filled with any existing draft answers.
        /// </summary>
        [HttpGet("questionnaire-assignments/{assignmentId:guid}/render")]
        [Authorize(Policy = "PatientOrDoctorOrAdmin")]
        public async Task<IActionResult> GetRender(Guid assignmentId, [FromQuery] Guid patientId)
        {
            var draft = await _service.GetDraftResponsesAsync(assignmentId, patientId);
            return Success(draft);
        }

        /// <summary>Patient views all questionnaires assigned to them.</summary>
        [HttpGet("patients/{patientId:guid}/questionnaire-assignments")]
        [Authorize]
        public async Task<IActionResult> GetPatientAssignments(
            Guid patientId, [FromQuery] PatientAssignmentFilterDto filter)
        {
            var result = await _service.GetPatientAssignmentsAsync(patientId, filter);
            return Success(result);
        }

        /// <summary>
        /// Returns existing draft answers for an assignment.
        /// Returns Pending status with empty answers if no draft exists yet.
        /// Angular uses this to pre-fill the form.
        /// </summary>
        [HttpGet("questionnaire-assignments/{assignmentId:guid}/draft")]
        [Authorize]
        public async Task<IActionResult> GetDraft(Guid assignmentId, [FromQuery] Guid patientId)
        {
            var result = await _service.GetDraftResponsesAsync(assignmentId, patientId);
            return Success(result);
        }

        /// <summary>
        /// Save current answers as a Draft.
        /// Can be called multiple times — each call replaces previous draft answers.
        /// Does NOT lock the submission.
        /// </summary>
        [HttpPost("questionnaire-assignments/{assignmentId:guid}/draft")]
        [Authorize]
        public async Task<IActionResult> SaveDraft(
            Guid assignmentId,
            [FromQuery] Guid patientId,
            [FromBody] SaveDraftRequestDto request)
        {
            var result = await _service.SaveDraftAsync(
                assignmentId, patientId, CurrentUserId, request);
            return Success(result, "Draft saved successfully.");
        }

        [HttpPost("questionnaire-assignments/{assignmentId:guid}/submit")]
        [Authorize]
        public async Task<IActionResult> Submit(
            Guid assignmentId,
            [FromQuery] Guid patientId,
            [FromBody] SubmitQuestionnaireRequestDto request)
        {
            var result = await _service.SubmitQuestionnaireAsync(
                assignmentId, patientId, CurrentUserId, request);
            return Created(result, "Questionnaire submitted successfully.");
        }

        [HttpGet("questionnaire-submissions/{submissionId:guid}")]
        [Authorize(Policy = "PatientOrDoctorOrAdmin")]
        public async Task<IActionResult> GetSubmissionDetail(Guid submissionId)
        {
            var result = await _service.GetSubmissionDetailAsync(submissionId);
            return Success(result);
        }

        [HttpGet("questionnaire-assignments/{assignmentId:guid}/versions")]
        [Authorize(Policy = "PatientOrDoctorOrAdmin")]
        public async Task<IActionResult> GetSubmissionVersions(Guid assignmentId)
        {
            var result = await _service.GetSubmissionVersionsAsync(assignmentId);
            return Success(result);
        }
    }
}
