using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class QuestionResponseItemDto
    {
        [Required(ErrorMessage = "Question ID is required.")]
        public Guid QuestionId { get; set; }
        public string? ResponseValue { get; set; }
        public List<string>? ResponseValues { get; set; }
    }
}