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
    }
}