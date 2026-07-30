namespace MediScope.Business.Services.Interfaces
{
    public interface IPdfService
    {
        Task<string> GenerateSubmissionPdfAsync(Guid submissionId, Guid patientId);
    }
}