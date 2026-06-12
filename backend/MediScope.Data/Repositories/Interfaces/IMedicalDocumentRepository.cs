using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IMedicalDocumentRepository
    {
        Task UploadDocumentAsync(MedicalDocument document);
        Task<List<MedicalDocumentResponseDto>> GetPatientDocumentsAsync(Guid patientId);
        Task<List<DoctorDocumentResponseDto>> GetDoctorDocumentsAsync(Guid doctorId);
        Task MarkViewedAsync(Guid documentId);
        Task AddFeedbackAsync(Guid documentId, string feedback, string? severity);
        Task<MedicalDocument?> GetDocumentByIdAsync(Guid documentId);
    }
}