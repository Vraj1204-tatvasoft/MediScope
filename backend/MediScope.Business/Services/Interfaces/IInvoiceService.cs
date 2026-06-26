using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Business.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<Guid> CreateInvoiceAsync(CreateInvoiceRequestDto dto);
        Task UpdateInvoiceAsync(Guid id, CreateInvoiceRequestDto dto);
        Task DeleteInvoiceAsync(Guid id);
        Task<List<InvoiceSummaryDto>> GetDoctorInvoicesAsync(Guid doctorId);
    }
}