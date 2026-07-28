using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class QuestionnaireQuestion : BaseEntity
    {
        public Guid QuestionnaireId { get; set; }
        public string Label { get; set; } = string.Empty;
        public FieldType FieldType { get; set; }
        public string? Placeholder { get; set; }
        public bool IsRequired { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string? DefaultValue { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public Questionnaire Questionnaire { get; set; } = null!;
        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public ICollection<SubmissionResponse> Responses { get; set; } = new List<SubmissionResponse>();
    }
}
