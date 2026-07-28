using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediScope.Common.Models.DTOs.Response
{
    public class DbQuestionRow
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string? Placeholder { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? DefaultValue { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public string Options { get; set; } = "[]";
    }
}