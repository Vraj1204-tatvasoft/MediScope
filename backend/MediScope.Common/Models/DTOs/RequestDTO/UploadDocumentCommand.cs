// File: MediScope.Common/Models/DTOs/Request/UploadDocumentCommand.cs

namespace MediScope.Common.Models.DTOs.Request
{
    /// <summary>
    /// Passed from Controller → Service after extracting raw file data from IFormFile.
    /// Business layer never touches IFormFile directly.
    /// </summary>
    public class UploadDocumentCommand
    {
        public Guid DoctorId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }

        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSizeBytes { get; set; }
    }
}