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

        public async Task<List<InvoiceSummaryDto>> GetDoctorInvoicesAsync(Guid doctorId)
        {
            _logger.LogInformation("Fetching invoices for Doctor {DoctorId}", doctorId);
            return await _repository.GetDoctorInvoicesAsync(doctorId);
        }
    }
}