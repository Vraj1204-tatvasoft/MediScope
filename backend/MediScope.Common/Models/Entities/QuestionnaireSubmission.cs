namespace MediScope.Common.Models.Entities
{
    public class QuestionnaireSubmission : BaseEntity
    {
        public Guid QuestionnaireId { get; set; }
        public Guid PatientId { get; set; }
        public Guid SubmittedBy { get; set; }
        public string? Notes { get; set; }
        public Questionnaire Questionnaire { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public ICollection<SubmissionResponse> Responses { get; set; } = new List<SubmissionResponse>();
    }
}
