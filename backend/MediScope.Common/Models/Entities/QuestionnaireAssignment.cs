namespace MediScope.Common.Models.Entities
{
    public class QuestionnaireAssignment : BaseEntity
    {
        public Guid QuestionnaireId { get; set; }
        public Guid PatientId { get; set; }
        public Guid AssignedBy { get; set; }
        public string? Notes { get; set; }
        public Questionnaire Questionnaire { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public ICollection<QuestionnaireSubmission> Submissions { get; set; } = new List<QuestionnaireSubmission>();
    }
}