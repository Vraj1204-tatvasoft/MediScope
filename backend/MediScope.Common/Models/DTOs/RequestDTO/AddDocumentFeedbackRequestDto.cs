using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.DTOs.Request
{
    public class AddDocumentFeedbackRequestDto
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public string Feedback { get; set; } = null!;

        public Severity? Severity { get; set; }
    }
}