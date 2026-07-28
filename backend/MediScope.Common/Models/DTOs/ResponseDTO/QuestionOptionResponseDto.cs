using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediScope.Common.Models.DTOs.Response
{
    public class QuestionOptionResponseDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}