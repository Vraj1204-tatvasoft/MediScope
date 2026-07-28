using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;

namespace MediScope.Business.Services.Interfaces
{
    public interface IQuestionnaireService
    {
        Task<Guid> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request, Guid userId);
        Task UpdateQuestionnaireAsync(Guid id, UpdateQuestionnaireRequestDto request, Guid userId);
        Task DeleteQuestionnaireAsync(Guid id, Guid userId);
        Task ToggleQuestionnaireStatusAsync(Guid id, Guid userId);
        Task<PagedResult<QuestionnaireListResponseDto>> GetQuestionnairesPagedAsync(QuestionnaireListFilterDto filter);
        Task<QuestionnaireDetailResponseDto> GetQuestionnaireByIdAsync(Guid id);
        Task<List<ActiveQuestionnaireResponseDto>> GetActiveQuestionnairesAsync();

        Task<Guid> AddQuestionAsync(Guid questionnaireId, CreateQuestionRequestDto request, Guid userId);
        Task UpdateQuestionAsync(Guid questionId, UpdateQuestionRequestDto request, Guid userId);
        Task DeleteQuestionAsync(Guid questionId, Guid userId);
        Task ReorderQuestionsAsync(Guid questionnaireId, ReorderQuestionsRequestDto request, Guid userId);
        Task<List<QuestionResponseDto>> GetQuestionsByQuestionnaireAsync(Guid questionnaireId);
    }
}