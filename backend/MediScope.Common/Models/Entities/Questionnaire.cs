using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Questionnaire : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Department { get; set; }
        public QuestionnaireStatus Status { get; set; } = QuestionnaireStatus.Active;
        public ICollection<QuestionnaireQuestion> Questions { get; set; } = new List<QuestionnaireQuestion>();
        public ICollection<QuestionnaireSubmission> Submissions { get; set; } = new List<QuestionnaireSubmission>();
    }
}
