using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediScope.Business.Services;
using MediScope.Business.Services.Interfaces;
using MediScope.Data.Repositories;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IInvoiceRepository repository, ILogger<InvoiceService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Guid> CreateInvoiceAsync(CreateInvoiceRequestDto dto)
        {
            _logger.LogInformation("Creating new invoice for Patient {PatientId}", dto.PatientId);
            return await _repository.CreateInvoiceAsync(dto);
        }

        public async Task UpdateInvoiceAsync(Guid id, CreateInvoiceRequestDto dto)
        {
            _logger.LogInformation("Updating invoice {InvoiceId}", id);
            await _repository.UpdateInvoiceAsync(id, dto);
        }

        public async Task DeleteInvoiceAsync(Guid id)
        {
            _logger.LogInformation("Attempting to delete invoice {InvoiceId}", id);
            await _repository.DeleteInvoiceAsync(id);
        }

        public async Task<List<InvoiceSummaryDto>> GetMyInvoicesAsync(Guid userId)
        {
            _logger.LogInformation("Fetching invoices for User {UserId}", userId);
            return await _repository.GetInvoicesByUserIdAsync(userId);
        }

        public async Task<List<BillingItemDto>> GetBillingItemsAsync()
        {
            _logger.LogInformation("Fetching billing items catalog.");
            return await _repository.GetBillingItemsAsync();
        }
        public async Task<InvoiceDetailsDto?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            _logger.LogInformation("Fetching full details for Invoice {InvoiceId}", invoiceId);
            return await _repository.GetInvoiceByIdAsync(invoiceId);
        }
        public async Task IssueRefundAsync(IssueRefundRequestDto dto, Guid currentUserId)
        {
            List<Guid> paymentIds;

            if (dto.PaymentIds != null && dto.PaymentIds.Any())
            {
                _logger.LogInformation("Issuing partial refund for {Count} payments on Invoice {InvoiceId}",
                    dto.PaymentIds.Count, dto.InvoiceId);
                paymentIds = dto.PaymentIds;
            }
            else
            {
                _logger.LogInformation("Issuing full refund for Invoice {InvoiceId}", dto.InvoiceId);
                paymentIds = await _repository.GetUnrefundedPaymentIdsAsync(dto.InvoiceId);

                if (!paymentIds.Any())
                    throw new InvalidOperationException("No valid payments found to refund on this invoice.");
            }

            var refundIds = paymentIds.Select(_ => Guid.NewGuid()).ToList();

            await _repository.IssueRefundAsync(
                refundIds, paymentIds, dto.InvoiceId,
                dto.RefundMode, dto.Reason, dto.RefundDate,
                dto.GrandTotal, currentUserId
            );
        }
        public async Task<Guid> GetPatientIdByUserIdAsync(Guid userId)
        {
            return await _repository.GetPatientIdByUserIdAsync(userId);
        }
        public async Task<string?> GetRazorpayCustomerIdAsync(Guid patientId)
        {
            return await _repository.GetRazorpayCustomerIdAsync(patientId);
        }
    }
}