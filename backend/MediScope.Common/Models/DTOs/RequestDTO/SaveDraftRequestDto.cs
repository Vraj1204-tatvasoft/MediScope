using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class SaveDraftRequestDto
    {
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Responses are required.")]
        public List<QuestionResponseItemDto> Responses { get; set; } = new();
    }
}