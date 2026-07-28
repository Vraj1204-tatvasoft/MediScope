using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Pagination;
using Npgsql;

namespace MediScope.API.Controllers.Features
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnairesController : BaseController
    {
        private readonly IQuestionnaireService _service;

        public QuestionnairesController(IQuestionnaireService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestionnaire([FromBody] CreateQuestionnaireRequestDto request)
        {
            var id = await _service.CreateQuestionnaireAsync(request, CurrentUserId);
            return Created(new { Id = id }, "Questionnaire created successfully.");
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateQuestionnaire(Guid id, [FromBody] UpdateQuestionnaireRequestDto request)
        {
            await _service.UpdateQuestionnaireAsync(id, request, CurrentUserId);
            return NoContent("Questionnaire updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteQuestionnaire(Guid id)
        {
            await _service.DeleteQuestionnaireAsync(id, CurrentUserId);
            return NoContent("Questionnaire deleted successfully.");
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            await _service.ToggleQuestionnaireStatusAsync(id, CurrentUserId);
            return NoContent("Questionnaire status toggled successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestionnaires([FromQuery] QuestionnaireListFilterDto filter)
        {
            var result = await _service.GetQuestionnairesPagedAsync(filter);
            return Success(result);
        }

        [HttpGet("{questionnaireId:guid}/questions")]
        public async Task<IActionResult> GetQuestionsByQuestionnaire(Guid questionnaireId)
        {
            var result = await _service.GetQuestionsByQuestionnaireAsync(questionnaireId);
            return Success(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetQuestionnaireById(Guid id)
        {
            var result = await _service.GetQuestionnaireByIdAsync(id);
            return Success(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveQuestionnaires()
        {
            var result = await _service.GetActiveQuestionnairesAsync();
            return Success(result);
        }

        [HttpPost("{questionnaireId:guid}/questions")]
        public async Task<IActionResult> AddQuestion(Guid questionnaireId, [FromBody] CreateQuestionRequestDto request)
        {
            var id = await _service.AddQuestionAsync(questionnaireId, request, CurrentUserId);
            return Created(new { Id = id }, "Question added successfully.");
        }

        [HttpPut("questions/{questionId:guid}")]
        public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromBody] UpdateQuestionRequestDto request)
        {
            await _service.UpdateQuestionAsync(questionId, request, CurrentUserId);
            return NoContent("Question updated successfully.");
        }

        [HttpDelete("questions/{questionId:guid}")]
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            await _service.DeleteQuestionAsync(questionId, CurrentUserId);
            return NoContent("Question deleted successfully.");
        }

        [HttpPatch("{questionnaireId:guid}/questions/reorder")]
        public async Task<IActionResult> ReorderQuestions(Guid questionnaireId, [FromBody] ReorderQuestionsRequestDto request)
        {
            await _service.ReorderQuestionsAsync(questionnaireId, request, CurrentUserId);
            return NoContent("Questions reordered successfully.");
        }

        // =====================================================================
        // PHASE 3 — SUBMISSION
        // =====================================================================

        // [HttpGet("{questionnaireId:guid}/render")]
        // public async Task<IActionResult> GetQuestionnaireRender(Guid questionnaireId)
        // {
        //     var result = await _service.GetQuestionnaireRenderAsync(questionnaireId);
        //     return Success(result);
        // }

        // [HttpPost("submit/{patientId:guid}")]
        // public async Task<IActionResult> SubmitQuestionnaire(Guid patientId, [FromBody] SubmitQuestionnaireRequestDto request)
        // {
        //     var submissionId = await _service.SubmitQuestionnaireAsync(patientId, request, CurrentUserId);
        //     return Created(new { SubmissionId = submissionId }, "Questionnaire submitted successfully.");
        // }

        // [HttpGet("submissions/patient/{patientId:guid}")]
        // public async Task<IActionResult> GetPatientSubmissions(Guid patientId, [FromQuery] PatientSubmissionListFilterDto filter)
        // {
        //     var result = await _service.GetPatientSubmissionHistoryAsync(patientId, filter);
        //     return Success(result);
        // }

        // [HttpGet("submissions/{submissionId:guid}")]
        // public async Task<IActionResult> GetSubmissionDetail(Guid submissionId)
        // {
        //     var result = await _service.GetSubmissionDetailAsync(submissionId);
        //     return Success(result);
        // }
    }
}