using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;
using MediScope.Data;
using MediScope.Common.Models.Entities;
namespace MediScope.Data.Repositories
{
    public class QuestionnaireAssignmentRepository : IQuestionnaireAssignmentRepository
    {
        private readonly AppDbContext _context;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public QuestionnaireAssignmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> AssignQuestionnaireAsync(
            AssignQuestionnaireRequestDto request, Guid assignedBy)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_assign_questionnaire(
                    {request.QuestionnaireId},
                    {request.PatientId},
                    {assignedBy},
                    {request.Notes},
                    NULL::uuid
                )");

            var newId = await _context.QuestionnaireAssignments
                .Where(a => a.QuestionnaireId == request.QuestionnaireId
                         && a.PatientId == request.PatientId
                         && a.AssignedBy == assignedBy
                         && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            return newId;
        }

        public async Task UnassignQuestionnaireAsync(Guid assignmentId, Guid deletedBy)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_unassign_questionnaire({assignmentId}, {deletedBy})");
        }

        public async Task<PagedResult<PatientAssignmentResponseDto>> GetPatientAssignmentsAsync(
            Guid patientId, PatientAssignmentFilterDto filter)
        {
            var rows = await _context.Database
                .SqlQuery<DbPatientAssignmentRow>($@"
                    SELECT * FROM fn_get_patient_assignments(
                        {patientId},
                        {filter.PageNumber},
                        {filter.PageSize},
                        {filter.Status},
                        {filter.AssignedBy}
                    )")
                .ToListAsync();

            var items = rows.Select(r => new PatientAssignmentResponseDto
            {
                AssignmentId = r.AssignmentId,
                QuestionnaireId = r.QuestionnaireId,
                QuestionnaireName = r.QuestionnaireName,
                Department = r.Department,
                AssignedByName = r.AssignedByName,
                AssignmentNotes = r.AssignmentNotes,
                AssignedAt = r.AssignedAt,
                FillStatus = r.FillStatus,
                SubmissionId = r.SubmissionId,
                SubmittedAt = r.SubmittedAt,
                PdfPath = r.PdfPath,
            }).ToList();

            return new PagedResult<PatientAssignmentResponseDto>
            {
                Items = items,
                TotalCount = (int)(rows.FirstOrDefault()?.TotalCount ?? 0),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            };
        }

        public async Task<PagedResult<DoctorAssignmentResponseDto>> GetDoctorAssignmentsAsync(
            Guid doctorId, DoctorAssignmentFilterDto filter)
        {
            var rows = await _context.Database
                .SqlQuery<DbDoctorAssignmentRow>($@"
                    SELECT * FROM fn_get_doctor_assignments(
                        {doctorId},
                        {filter.PatientId},
                        {filter.PageNumber},
                        {filter.PageSize}
                    )")
                .ToListAsync();

            var items = rows.Select(r => new DoctorAssignmentResponseDto
            {
                AssignmentId = r.AssignmentId,
                QuestionnaireId = r.QuestionnaireId,
                QuestionnaireName = r.QuestionnaireName,
                PatientId = r.PatientId,
                PatientName = r.PatientName,
                AssignmentNotes = r.AssignmentNotes,
                AssignedAt = r.AssignedAt,
                FillStatus = r.FillStatus,
                SubmissionId = r.SubmissionId,
                SubmittedAt = r.SubmittedAt,
            }).ToList();

            return new PagedResult<DoctorAssignmentResponseDto>
            {
                Items = items,
                TotalCount = (int)(rows.FirstOrDefault()?.TotalCount ?? 0),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            };
        }

        // ── Get Draft (pre-fill) ──────────────────────────────────────────────
        public async Task<DraftResponseDto> GetDraftResponsesAsync(Guid assignmentId, Guid patientId)
        {
            var rows = await _context.Database
                .SqlQuery<DbDraftResponseRow>($@"
                    SELECT * FROM fn_get_draft_responses({assignmentId}, {patientId})")
                .ToListAsync();

            // No draft yet — return Pending shell
            if (!rows.Any() || rows.First().SubmissionId is null)
            {
                return new DraftResponseDto { Status = "Pending" };
            }

            var first = rows.First();
            return new DraftResponseDto
            {
                SubmissionId = first.SubmissionId,
                Status = first.Status ?? "Draft",
                Notes = first.Notes,
                Answers = rows
                    .Where(r => r.QuestionId.HasValue)
                    .Select(r => new DraftAnswerItemDto
                    {
                        QuestionId = r.QuestionId!.Value,
                        ResponseValue = r.ResponseValue,
                        ResponseValues = r.ResponseValues?.ToList(),
                    }).ToList()
            };
        }

        // ── Save Draft ────────────────────────────────────────────────────────
        public async Task<SaveDraftResultDto> SaveDraftAsync(
            Guid assignmentId, Guid patientId, Guid userId, SaveDraftRequestDto request)
        {
            var responsesJson = JsonSerializer.Serialize(
                request.Responses.Select(r => new
                {
                    questionId = r.QuestionId,
                    responseValue = r.ResponseValue,
                    responseValues = r.ResponseValues,
                }));

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_save_draft(
                    {assignmentId},
                    {patientId},
                    {userId},
                    {request.Notes},
                    {responsesJson}::jsonb,
                    NULL::uuid
                )");

            // Fetch the submission id
            var submissionId = await _context.QuestionnaireSubmissions
                .Where(s => s.AssignmentId == assignmentId
                         && s.PatientId == patientId
                         && !s.IsDeleted)
                .OrderByDescending(s => s.UpdatedAt)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            return new SaveDraftResultDto
            {
                SubmissionId = submissionId,
                Status = "Draft",
            };
        }

        // ── Submit ────────────────────────────────────────────────────────────
        public async Task<SubmitResultDto> SubmitQuestionnaireAsync(
            Guid assignmentId, Guid patientId, Guid userId, SubmitQuestionnaireRequestDto request)
        {
            var responsesJson = JsonSerializer.Serialize(
                request.Responses.Select(r => new
                {
                    questionId = r.QuestionId,
                    responseValue = r.ResponseValue,
                    responseValues = r.ResponseValues,
                }));

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_submit_questionnaire(
                    {assignmentId},
                    {patientId},
                    {userId},
                    {request.Notes},
                    {responsesJson}::jsonb,
                    NULL::uuid
                )");

            var submission = await _context.QuestionnaireSubmissions
                .Where(s => s.AssignmentId == assignmentId
                         && s.PatientId == patientId
                         && s.Status == "Submitted"
                         && !s.IsDeleted)
                .OrderByDescending(s => s.UpdatedAt)
                .Select(s => new { s.Id, s.PdfPath })
                .FirstOrDefaultAsync();

            return new SubmitResultDto
            {
                SubmissionId = submission?.Id ?? Guid.Empty,
                Status = "Submitted",
                PdfPath = submission?.PdfPath,
            };
        }

        public async Task UpdatePdfPathAsync(Guid submissionId, string pdfPath)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_update_pdf_path({submissionId}, {pdfPath})");
        }
        public async Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid assignmentId)
        {
            return await _context.QuestionnaireAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
        }
        public async Task<SubmissionDetailResponseDto?> GetSubmissionDetailAsync(Guid submissionId)
        {
            var row = await _context.Database
                .SqlQuery<DbSubmissionDetailRow>($@"
                    SELECT * FROM fn_get_submission_detail({submissionId})")
                .FirstOrDefaultAsync();

            if (row is null) return null;

            var responseItems = JsonSerializer.Deserialize<List<DbResponseJsonItem>>(
                row.Responses, _jsonOpts) ?? new();

            return new SubmissionDetailResponseDto
            {
                SubmissionId = row.SubmissionId,
                AssignmentId = row.AssignmentId,
                QuestionnaireId = row.QuestionnaireId,
                QuestionnaireName = row.QuestionnaireName,
                Department = row.Department,
                Status = row.Status,
                SubmittedAt = row.SubmittedAt,
                SubmittedByName = row.SubmittedByName,
                Notes = row.Notes,
                PdfPath = row.PdfPath,
                Responses = responseItems.Select(r => new SubmissionResponseItemDto
                {
                    QuestionId = r.QuestionId,
                    Label = r.Label,
                    FieldType = r.FieldType,
                    DisplayOrder = r.DisplayOrder,
                    ResponseValue = r.ResponseValue,
                    ResponseValues = r.ResponseValues,
                }).ToList()
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────
        private class DbResponseJsonItem
        {
            public Guid QuestionId { get; set; }
            public string Label { get; set; } = string.Empty;
            public string FieldType { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public string? ResponseValue { get; set; }
            public List<string>? ResponseValues { get; set; }
        }
    }
}
