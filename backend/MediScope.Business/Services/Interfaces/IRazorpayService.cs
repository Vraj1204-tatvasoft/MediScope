using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IRazorpayService
    {
        Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid invoiceId, decimal amount, string? razorpayCustomerId, string? patientContact, string? patientEmail);

        bool VerifySignature(string orderId, string paymentId, string signature);
        Task<string?> FetchAndSaveCardTokenAsync(string razorpayPaymentId, Guid patientId);

        Task<string> GetOrCreateCustomerAsync(Guid patientId, string patientName, string? email, string? contact);
        Task<string> GetCustomerTokensAsync(string customerId);
        // Task<string> ChargeSavedCardAsync(string tokenId, string customerId, Guid invoiceId, decimal amount);
    }
}