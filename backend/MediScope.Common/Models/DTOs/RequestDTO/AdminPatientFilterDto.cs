using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace MediScope.Common.Models.DTOs.Request
{
    public class AdminPatientFilterDto
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
    }

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