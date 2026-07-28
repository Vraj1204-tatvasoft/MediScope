using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateQuestionRequestDto
    {
        [Required(ErrorMessage = "Label is required.")]
        [MaxLength(500, ErrorMessage = "Label must not exceed 500 characters.")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field type is required.")]
        [RegularExpression("^(TextBox|TextArea|Number|Date|Dropdown|RadioButton|Checkbox)$", ErrorMessage = "Invalid field type.")]
        public string FieldType { get; set; } = string.Empty;

        [MaxLength(300, ErrorMessage = "Placeholder must not exceed 300 characters.")]
        public string? Placeholder { get; set; }

        public bool IsRequired { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public string? DefaultValue { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public List<QuestionOptionRequestDto>? Options { get; set; }
    }
}