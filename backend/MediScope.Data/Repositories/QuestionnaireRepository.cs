using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data;

namespace MediScope.Data.Repositories
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly AppDbContext _context;
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public QuestionnaireRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request, Guid userId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_create_questionnaire(
                    {request.Name},
                    {request.Description},
                    {request.Department},
                    {request.Status},
                    {userId},
                    NULL::uuid
                )");

            var newId = await _context.Questionnaires
                .Where(q => q.CreatedBy == userId && !q.IsDeleted)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => q.Id)
                .FirstOrDefaultAsync();

            return newId;
        }
        public async Task UpdateQuestionnaireAsync(Guid id, UpdateQuestionnaireRequestDto request, Guid userId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_update_questionnaire(
                    {id},
                    {request.Name},
                    {request.Description},
                    {request.Department},
                    {request.Status},
                    {userId}
                )");
        }
        public async Task DeleteQuestionnaireAsync(Guid id, Guid userId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"CALL sp_delete_questionnaire({id}, {userId})");
        }
        public async Task ToggleQuestionnaireStatusAsync(Guid id, Guid userId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"CALL sp_toggle_questionnaire_status({id}, {userId})");
        }
        public async Task<PagedResult<QuestionnaireListResponseDto>> GetQuestionnairesPagedAsync(QuestionnaireListFilterDto filter)
        {
            var dbRows = await _context.Database
                .SqlQuery<DbQuestionnaireListRow>($@"
                    SELECT * FROM fn_get_questionnaires_paged(
                        {filter.Search},
                        {filter.Status},
                        {filter.PageNumber},
                        {filter.PageSize}
                    )")
                .ToListAsync();

            var items = dbRows.Select(r => new QuestionnaireListResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Department = r.Department,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                QuestionCount = r.QuestionCount
            }).ToList();

            return new PagedResult<QuestionnaireListResponseDto>
            {
                Items = items,
                TotalCount = (int)(dbRows.FirstOrDefault()?.TotalCount ?? 0),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
        public async Task<QuestionnaireDetailResponseDto?> GetQuestionnaireByIdAsync(Guid id)
        {
            var dbRow = await _context.Database
                .SqlQuery<DbQuestionnaireDetailRow>($@"SELECT * FROM fn_get_questionnaire_by_id({id})")
                .FirstOrDefaultAsync();

            if (dbRow is null) return null;

            var questions = await GetQuestionsByQuestionnaireAsync(id);

            return new QuestionnaireDetailResponseDto
            {
                Id = dbRow.Id,
                Name = dbRow.Name,
                Description = dbRow.Description,
                Department = dbRow.Department,
                Status = dbRow.Status,
                CreatedAt = dbRow.CreatedAt,
                UpdatedAt = dbRow.UpdatedAt,
                Questions = questions
            };
        }
        public async Task<List<ActiveQuestionnaireResponseDto>> GetActiveQuestionnairesAsync()
        {
            var dbRows = await _context.Database
                .SqlQuery<DbActiveQuestionnaireRow>($@"SELECT * FROM fn_get_active_questionnaires()")
                .ToListAsync();

            return dbRows.Select(r => new ActiveQuestionnaireResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Department = r.Department
            }).ToList();
        }
        public async Task<Guid> AddQuestionAsync(Guid questionnaireId, CreateQuestionRequestDto request, Guid userId)
        {
            var optionsJson = request.Options != null
                ? JsonSerializer.Serialize(request.Options.Select(o => new
                { label = o.Label, value = o.Value, order = o.Order }))
                : null;

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_add_question(
                    {questionnaireId},
                    {request.Label},
                    {request.FieldType},
                    {request.Placeholder},
                    {request.IsRequired},
                    {request.DisplayOrder},
                    {request.DefaultValue},
                    {request.MinValue},        
                    {request.MaxValue},        
                    {request.MinLength},       
                    {request.MaxLength},       
                    {request.RegexPattern},
                    {userId},
                    {optionsJson}::jsonb,
                    NULL::uuid
                )");

            var newId = await _context.QuestionnaireQuestions
                .Where(q => q.QuestionnaireId == questionnaireId
                         && q.CreatedBy == userId
                         && !q.IsDeleted)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => q.Id)
                .FirstOrDefaultAsync();

            return newId;
        }
        public async Task UpdateQuestionAsync(Guid questionId, UpdateQuestionRequestDto request, Guid userId)
        {
            var optionsJson = request.Options != null
                ? JsonSerializer.Serialize(request.Options.Select(o => new
                { label = o.Label, value = o.Value, order = o.Order }))
                : null;

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_update_question(
                    {questionId},
                    {request.Label},
                    {request.FieldType},
                    {request.Placeholder},
                    {request.IsRequired},
                    {request.DisplayOrder},
                    {request.DefaultValue},
                    {request.MinValue},        
                    {request.MaxValue},        
                    {request.MinLength},       
                    {request.MaxLength},       
                    {request.RegexPattern},
                    {userId},
                    {optionsJson}::jsonb
                )");
        }
        public async Task DeleteQuestionAsync(Guid questionId, Guid userId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"CALL sp_delete_question({questionId}, {userId})");
        }
        public async Task ReorderQuestionsAsync(Guid questionnaireId, ReorderQuestionsRequestDto request, Guid userId)
        {
            var orderMapJson = JsonSerializer.Serialize(request.OrderMap.Select(o => new { id = o.Id, order = o.Order }));

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_reorder_questions(
                    {questionnaireId},
                    {orderMapJson}::jsonb,
                    {userId}
                )");
        }
        public async Task<List<QuestionResponseDto>> GetQuestionsByQuestionnaireAsync(Guid questionnaireId)
        {
            var dbRows = await _context.Database
                .SqlQuery<DbQuestionRow>($@"SELECT * FROM fn_get_questions_by_questionnaire({questionnaireId})")
                .ToListAsync();

            return dbRows.Select(r => MapToQuestionResponseDto(r)).ToList();
        }
        private static QuestionResponseDto MapToQuestionResponseDto(DbQuestionRow r)
        {
            return new QuestionResponseDto
            {
                Id = r.Id,
                Label = r.Label,
                FieldType = r.FieldType,
                Placeholder = r.Placeholder,
                IsRequired = r.IsRequired,
                DisplayOrder = r.DisplayOrder,
                DefaultValue = r.DefaultValue,
                MinValue = r.MinValue,
                MaxValue = r.MaxValue,
                MinLength = r.MinLength,
                MaxLength = r.MaxLength,
                RegexPattern = r.RegexPattern,
                Options = ParseOptions(r.Options)
            };
        }

        private static List<QuestionOptionResponseDto> ParseOptions(string optionsJson)
        {
            if (string.IsNullOrWhiteSpace(optionsJson) || optionsJson == "[]")
                return new List<QuestionOptionResponseDto>();

            var raw = JsonSerializer.Deserialize<List<DbOptionJsonItem>>(optionsJson, _jsonOpts);
            return raw?.Select(o => new QuestionOptionResponseDto
            {
                Id = o.Id,
                Label = o.Label,
                Value = o.Value,
                DisplayOrder = o.DisplayOrder
            }).ToList() ?? new();
        }

        private class DbOptionJsonItem
        {
            public Guid Id { get; set; }
            public string Label { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
        }
    }
}