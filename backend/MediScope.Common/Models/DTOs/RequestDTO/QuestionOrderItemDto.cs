using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class QuestionOrderItemDto
    {
        [Required]
        public Guid Id { get; set; }
        public int Order { get; set; }
        // public int? MinValue { get; set; }
        // public int? MaxValue { get; set; }
        // public int? MinLength { get; set; }
        // public int? MaxLength { get; set; }
        // public string? RegexPattern { get; set; }
    }
}