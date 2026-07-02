using MediScope.Common.Models.Entities;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.DTOs.Request;
namespace MediScope.Data.Repositories
{
    public interface IInvoiceRepository
    {
        Task<Guid> CreateInvoiceAsync(CreateInvoiceRequestDto dto);
        Task UpdateInvoiceAsync(Guid invoiceId, CreateInvoiceRequestDto dto);
        Task DeleteInvoiceAsync(Guid invoiceId);
        Task<List<DoctorInvoiceSummaryDto>> GetDoctorInvoicesAsync(Guid doctorId);
        Task<List<BillingItemDto>> GetBillingItemsAsync();
        Task<InvoiceDetailsDto?> GetInvoiceByIdAsync(Guid invoiceId);
        Task<List<PatientInvoiceSummaryDto>> GetPatientInvoicesAsync(Guid patientId);
    }
}