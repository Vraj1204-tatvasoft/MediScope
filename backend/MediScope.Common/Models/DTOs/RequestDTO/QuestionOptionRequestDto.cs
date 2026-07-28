using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class QuestionOptionRequestDto
    {
        [Required(ErrorMessage = "Option label is required.")]
        [MaxLength(300, ErrorMessage = "Option label must not exceed 300 characters.")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Option value is required.")]
        [MaxLength(300, ErrorMessage = "Option value must not exceed 300 characters.")]
        public string Value { get; set; } = string.Empty;

        public int Order { get; set; } = 0;
    }
}