namespace MediScope.Common.Models.Entities
{
    public class QuestionnaireSubmission : BaseEntity
    {
        public Guid QuestionnaireId { get; set; }
        public Guid PatientId { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid SubmittedBy { get; set; }
        public string Status { get; set; } = "Draft";
        public string? Notes { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? PdfPath { get; set; }
        public int VersionNumber { get; set; } = 1;
        public Questionnaire Questionnaire { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public QuestionnaireAssignment? Assignment { get; set; }
        public ICollection<SubmissionResponse> Responses { get; set; } = new List<SubmissionResponse>();
    }
}