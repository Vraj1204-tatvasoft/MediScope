using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediScope.Common.Models.DTOs.Response
{
    public class QuestionnaireListResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Department { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long QuestionCount { get; set; }
    }
}