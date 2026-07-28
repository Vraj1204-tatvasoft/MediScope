using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IQuestionnaireRepository _repository;

        public QuestionnaireService(IQuestionnaireRepository repository)
        {
            _repository = repository;
        }
        public async Task<Guid> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request, Guid userId)
        {
            return await _repository.CreateQuestionnaireAsync(request, userId);
        }

        public async Task UpdateQuestionnaireAsync(Guid id, UpdateQuestionnaireRequestDto request, Guid userId)
        {
            await _repository.UpdateQuestionnaireAsync(id, request, userId);
        }

        public async Task DeleteQuestionnaireAsync(Guid id, Guid userId)
        {
            await _repository.DeleteQuestionnaireAsync(id, userId);
        }

        public async Task ToggleQuestionnaireStatusAsync(Guid id, Guid userId)
        {
            await _repository.ToggleQuestionnaireStatusAsync(id, userId);
        }

        public async Task<PagedResult<QuestionnaireListResponseDto>> GetQuestionnairesPagedAsync(QuestionnaireListFilterDto filter)
        {
            return await _repository.GetQuestionnairesPagedAsync(filter);
        }

        public async Task<QuestionnaireDetailResponseDto> GetQuestionnaireByIdAsync(Guid id)
        {
            var questionnaire = await _repository.GetQuestionnaireByIdAsync(id);
            if (questionnaire == null)
            {
                // Let the GlobalExceptionMiddleware catch this and return 404
                throw new KeyNotFoundException("Questionnaire not found.");
            }
            return questionnaire;
        }

        public async Task<List<ActiveQuestionnaireResponseDto>> GetActiveQuestionnairesAsync()
        {
            return await _repository.GetActiveQuestionnairesAsync();
        }

        public async Task<Guid> AddQuestionAsync(Guid questionnaireId, CreateQuestionRequestDto request, Guid userId)
        {
            ValidateQuestionOptions(request.FieldType, request.Options);
            return await _repository.AddQuestionAsync(questionnaireId, request, userId);
        }

        public async Task UpdateQuestionAsync(Guid questionId, UpdateQuestionRequestDto request, Guid userId)
        {
            ValidateQuestionOptions(request.FieldType, request.Options);
            await _repository.UpdateQuestionAsync(questionId, request, userId);
        }

        public async Task DeleteQuestionAsync(Guid questionId, Guid userId)
        {
            await _repository.DeleteQuestionAsync(questionId, userId);
        }

        public async Task ReorderQuestionsAsync(Guid questionnaireId, ReorderQuestionsRequestDto request, Guid userId)
        {
            await _repository.ReorderQuestionsAsync(questionnaireId, request, userId);
        }

        public async Task<List<QuestionResponseDto>> GetQuestionsByQuestionnaireAsync(Guid questionnaireId)
        {
            return await _repository.GetQuestionsByQuestionnaireAsync(questionnaireId);
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 3 — Submission
        // ─────────────────────────────────────────────────────────────

        // public async Task<QuestionnaireRenderResponseDto> GetQuestionnaireRenderAsync(Guid questionnaireId)
        // {
        //     return await _repository.GetQuestionnaireRenderAsync(questionnaireId);
        // }

        // public async Task<Guid> SubmitQuestionnaireAsync(Guid patientId, SubmitQuestionnaireRequestDto request, Guid userId)
        // {
        //     if (request.Responses == null || !request.Responses.Any())
        //     {
        //         throw new ArgumentException("At least one response is required to submit a questionnaire.");
        //     }

        //     return await _repository.SubmitQuestionnaireAsync(patientId, request, userId);
        // }

        // public async Task<PagedResult<SubmissionHistoryResponseDto>> GetPatientSubmissionHistoryAsync(Guid patientId, PatientSubmissionListFilterDto filter)
        // {
        //     return await _repository.GetPatientSubmissionHistoryAsync(patientId, filter);
        // }

        // public async Task<SubmissionDetailResponseDto> GetSubmissionDetailAsync(Guid submissionId)
        // {
        //     var submission = await _repository.GetSubmissionDetailAsync(submissionId);
        //     if (submission == null)
        //     {
        //         throw new KeyNotFoundException("Submission details not found.");
        //     }
        //     return submission;
        // }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        private static void ValidateQuestionOptions(string fieldType, List<QuestionOptionRequestDto>? options)
        {
            var requiresOptions = fieldType is "Dropdown" or "RadioButton" or "Checkbox";

            if (requiresOptions && (options == null || options.Count < 2))
            {
                throw new ArgumentException($"Field type '{fieldType}' requires at least 2 options.");
            }
        }
    }
}