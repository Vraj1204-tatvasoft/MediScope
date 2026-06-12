using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class AddDocumentFeedbackRequestDto
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public string Feedback { get; set; } = null!;

        public string? Severity { get; set; }
    }
}