using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediScope.Common.Models.DTOs.Response
{
    public class DbRenderRow
    {
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Department { get; set; }
        public Guid QuestionId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string? Placeholder { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? DefaultValue { get; set; }
        public string Options { get; set; } = "[]";
    }
}