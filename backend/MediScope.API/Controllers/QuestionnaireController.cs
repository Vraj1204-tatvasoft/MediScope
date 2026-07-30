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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateQuestionnaire([FromBody] CreateQuestionnaireRequestDto request)
        {
            var id = await _service.CreateQuestionnaireAsync(request, CurrentUserId);
            return Created(new { Id = id }, "Questionnaire created successfully.");
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateQuestionnaire(Guid id, [FromBody] UpdateQuestionnaireRequestDto request)
        {
            await _service.UpdateQuestionnaireAsync(id, request, CurrentUserId);
            return NoContent("Questionnaire updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteQuestionnaire(Guid id)
        {
            await _service.DeleteQuestionnaireAsync(id, CurrentUserId);
            return NoContent("Questionnaire deleted successfully.");
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            await _service.ToggleQuestionnaireStatusAsync(id, CurrentUserId);
            return NoContent("Questionnaire status toggled successfully.");
        }

        [HttpGet]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> GetQuestionnaires([FromQuery] QuestionnaireListFilterDto filter)
        {
            var result = await _service.GetQuestionnairesPagedAsync(filter);
            return Success(result);
        }

        [HttpGet("{questionnaireId:guid}/questions")]
        [Authorize(Policy = "PatientOrDoctorOrAdmin")]
        public async Task<IActionResult> GetQuestionsByQuestionnaire(Guid questionnaireId)
        {
            var result = await _service.GetQuestionsByQuestionnaireAsync(questionnaireId);
            return Success(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "PatientOrAdmin")]
        public async Task<IActionResult> GetQuestionnaireById(Guid id)
        {
            var result = await _service.GetQuestionnaireByIdAsync(id);
            return Success(result);
        }

        [HttpGet("active")]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> GetActiveQuestionnaires()
        {
            var result = await _service.GetActiveQuestionnairesAsync();
            return Success(result);
        }

        [HttpPost("{questionnaireId:guid}/questions")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddQuestion(Guid questionnaireId, [FromBody] CreateQuestionRequestDto request)
        {
            var id = await _service.AddQuestionAsync(questionnaireId, request, CurrentUserId);
            return Created(new { Id = id }, "Question added successfully.");
        }

        [HttpPut("questions/{questionId:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromBody] UpdateQuestionRequestDto request)
        {
            await _service.UpdateQuestionAsync(questionId, request, CurrentUserId);
            return NoContent("Question updated successfully.");
        }

        [HttpDelete("questions/{questionId:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            await _service.DeleteQuestionAsync(questionId, CurrentUserId);
            return NoContent("Question deleted successfully.");
        }

        [HttpPatch("{questionnaireId:guid}/questions/reorder")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ReorderQuestions(Guid questionnaireId, [FromBody] ReorderQuestionsRequestDto request)
        {
            await _service.ReorderQuestionsAsync(questionnaireId, request, CurrentUserId);
            return NoContent("Questions reordered successfully.");
        }
    }
}