using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class ReorderQuestionsRequestDto
    {
        [Required]
        public List<QuestionOrderItemDto> OrderMap { get; set; } = new();
    }
}