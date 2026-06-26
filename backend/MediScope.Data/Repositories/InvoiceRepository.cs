using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore;
using MediScope.Data.Repositories;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Data.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateInvoiceAsync(CreateInvoiceRequestDto dto)
        {
            var newInvoiceId = Guid.NewGuid();
            string itemsJson = JsonSerializer.Serialize(dto.Items);

            var pId = new NpgsqlParameter("@p_id", newInvoiceId);
            var pAppt = new NpgsqlParameter("@p_appointment_id", dto.AppointmentId);
            var pDoc = new NpgsqlParameter("@p_doctor_id", dto.DoctorId);
            var pPat = new NpgsqlParameter("@p_patient_id", dto.PatientId);
            var pSub = new NpgsqlParameter("@p_sub_total", dto.SubTotal);
            var pDis = new NpgsqlParameter("@p_total_discount", dto.TotalDiscount);
            var pTax = new NpgsqlParameter("@p_total_tax", dto.TotalTax);
            var pGrand = new NpgsqlParameter("@p_grand_total", dto.GrandTotal);
            var tPaid = new NpgsqlParameter("@p_total_paid", dto.TotalPaid);
            var pItems = new NpgsqlParameter("@p_items_json", NpgsqlDbType.Jsonb)
            {
                Value = itemsJson
            };

            await _context.Database.ExecuteSqlRawAsync(
                "CALL sp_create_invoice(@p_id, @p_appointment_id, @p_doctor_id, @p_patient_id, @p_sub_total, @p_total_discount, @p_total_tax, @p_grand_total, @p_total_paid, @p_items_json)",
                pId, pAppt, pDoc, pPat, pSub, pDis, pTax, pGrand, tPaid, pItems
            );

            return newInvoiceId;
        }

        public async Task UpdateInvoiceAsync(Guid invoiceId, CreateInvoiceRequestDto dto)
        {
            string itemsJson = JsonSerializer.Serialize(dto.Items);

            await _context.Database.ExecuteSqlRawAsync(
                "CALL sp_update_invoice({0}, {1}, {2}, {3}, {4}, {5}, {6}::jsonb)",
                invoiceId, dto.SubTotal, dto.TotalDiscount, dto.TotalTax, dto.GrandTotal, dto.TotalPaid, itemsJson
            );
        }

        public async Task DeleteInvoiceAsync(Guid invoiceId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "CALL sp_delete_invoice({0})",
                invoiceId
            );
        }

        public async Task<List<InvoiceSummaryDto>> GetDoctorInvoicesAsync(Guid doctorId)
        {
            return await _context.Database
                .SqlQueryRaw<InvoiceSummaryDto>("SELECT * FROM fn_get_invoices_by_doctor({0})", doctorId)
                .ToListAsync();
        }
    }
}