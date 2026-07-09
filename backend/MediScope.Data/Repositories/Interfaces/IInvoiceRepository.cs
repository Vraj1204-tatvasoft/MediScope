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
        Task<List<BillingItemDto>> GetBillingItemsAsync();
        Task<InvoiceDetailsDto?> GetInvoiceByIdAsync(Guid invoiceId);
        Task<List<InvoiceSummaryDto>> GetInvoicesByUserIdAsync(Guid userId);
        Task IssueRefundAsync(List<Guid> refundIds, List<Guid> paymentIds, Guid invoiceId, string refundMode, string? reason, DateTime refundDate, decimal grandTotal, Guid? createdBy);
        Task<List<Guid>> GetUnrefundedPaymentIdsAsync(Guid invoiceId);
        Task<Guid> GetPatientIdByUserIdAsync(Guid userId);
        Task SaveCardTokenAsync(Guid patientId, string tokenId, string last4, string network);
        Task<string?> GetRazorpayCustomerIdAsync(Guid patientId);
        Task SaveRazorpayCustomerIdAsync(Guid patientId, string customerId);
    }
}