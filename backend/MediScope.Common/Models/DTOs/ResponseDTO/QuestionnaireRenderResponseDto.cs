using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediScope.Common.Models.DTOs.Response
{
    public class QuestionnaireRenderResponseDto
    {
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Department { get; set; }
        public List<QuestionResponseDto> Questions { get; set; } = new();
    }
}