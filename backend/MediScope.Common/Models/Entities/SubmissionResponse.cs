namespace MediScope.Common.Models.Entities
{
    public class SubmissionResponse
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid QuestionId { get; set; }
        public string? ResponseValue { get; set; }
        public string[]? ResponseValues { get; set; }
        public QuestionnaireSubmission Submission { get; set; } = null!;
        public QuestionnaireQuestion Question { get; set; } = null!;
    }
}
