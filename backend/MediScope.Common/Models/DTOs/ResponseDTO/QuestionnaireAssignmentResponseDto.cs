namespace MediScope.Common.Models.DTOs.Response
{
    public class DbPatientAssignmentRow
    {
        public Guid AssignmentId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        public string? AssignmentNotes { get; set; }
        public DateTime AssignedAt { get; set; }
        public string FillStatus { get; set; } = "Pending";
        public Guid? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? PdfPath { get; set; }
        public long TotalCount { get; set; }
    }

    public class DbDoctorAssignmentRow
    {
        public Guid AssignmentId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? AssignmentNotes { get; set; }
        public DateTime AssignedAt { get; set; }
        public string FillStatus { get; set; } = "Pending";
        public Guid? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public long TotalCount { get; set; }
    }

    public class DbDraftResponseRow
    {
        public Guid? SubmissionId { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public Guid? QuestionId { get; set; }
        public string? ResponseValue { get; set; }
        public string[]? ResponseValues { get; set; }
    }

    public class DbSubmissionDetailRow
    {
        public Guid SubmissionId { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public string SubmittedByName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? PdfPath { get; set; }
        public string Responses { get; set; } = "[]";
    }

    public class PatientAssignmentResponseDto
    {
        public Guid AssignmentId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        public string? AssignmentNotes { get; set; }
        public DateTime AssignedAt { get; set; }
        public string FillStatus { get; set; } = "Pending";
        public Guid? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? PdfPath { get; set; }
    }
    public class DoctorAssignmentResponseDto
    {
        public Guid AssignmentId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? AssignmentNotes { get; set; }
        public DateTime AssignedAt { get; set; }
        public string FillStatus { get; set; } = "Pending";
        public Guid? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class DraftResponseDto
    {
        public Guid? SubmissionId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public List<DraftAnswerItemDto> Answers { get; set; } = new();
    }

    public class DraftAnswerItemDto
    {
        public Guid QuestionId { get; set; }
        public string? ResponseValue { get; set; }
        public List<string>? ResponseValues { get; set; }
    }

    public class SaveDraftResultDto
    {
        public Guid SubmissionId { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public class SubmitResultDto
    {
        public Guid SubmissionId { get; set; }
        public string Status { get; set; } = "Submitted";
        public string? PdfPath { get; set; }
    }

    public class SubmissionDetailResponseDto
    {
        public Guid SubmissionId { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public string SubmittedByName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? PdfPath { get; set; }
        public List<SubmissionResponseItemDto> Responses { get; set; } = new();
    }

    public class SubmissionResponseItemDto
    {
        public Guid QuestionId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? ResponseValue { get; set; }
        public List<string>? ResponseValues { get; set; }
    }
}
