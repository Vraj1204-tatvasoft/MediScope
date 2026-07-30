using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class SubmitQuestionnaireRequestDto
    {
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Responses are required.")]
        [MinLength(1, ErrorMessage = "At least one response is required.")]
        public List<QuestionResponseItemDto> Responses { get; set; } = new();
    }
}