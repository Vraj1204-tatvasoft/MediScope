using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class UploadDocumentRequestDto
    {
        [Required]
        public Guid DoctorId { get; set; }
        [Required]
        public IFormFile File { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }
    }
}