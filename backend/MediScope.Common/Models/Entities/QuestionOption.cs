namespace MediScope.Common.Models.Entities
{
    public class QuestionOption
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string OptionLabel { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;
        public QuestionnaireQuestion Question { get; set; } = null!;
    }
}
