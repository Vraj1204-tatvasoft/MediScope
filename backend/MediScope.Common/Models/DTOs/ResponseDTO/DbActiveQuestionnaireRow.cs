using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediScope.Common.Models.DTOs.Response
{
    public class DbActiveQuestionnaireRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Department { get; set; }
    }
}