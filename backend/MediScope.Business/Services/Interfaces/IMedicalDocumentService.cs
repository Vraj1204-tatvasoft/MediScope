using Microsoft.AspNetCore.Http;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IMedicalDocumentService
    {
        Task UploadAsync(UploadDocumentCommand request, Guid patientUserId);

        Task<List<MedicalDocumentResponseDto>> GetPatientDocumentsAsync(Guid patientUserId);

        Task<List<DoctorDocumentResponseDto>> GetDoctorDocumentsAsync(Guid doctorUserId);

        Task AddFeedbackAsync(Guid doctorUserId, AddDocumentFeedbackRequestDto request);

        Task MarkViewedAsync(Guid doctorUserId, Guid documentId);

        Task<(FileStream Content, string ContentType, string FileName)> DownloadAsync(Guid userId, string role, Guid documentId);
    }
}