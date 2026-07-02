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
            string paymentsJson = JsonSerializer.Serialize(dto.Payments);
            await _context.Database.ExecuteSqlRawAsync(
                "CALL sp_update_invoice({0}, {1}, {2}, {3}, {4}, {5}::jsonb, {6}::jsonb)",
                invoiceId, dto.SubTotal, dto.TotalDiscount, dto.TotalTax, dto.GrandTotal, itemsJson, paymentsJson
            );
        }

        public async Task DeleteInvoiceAsync(Guid invoiceId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "CALL sp_delete_invoice({0})",
                invoiceId
            );
        }

        public async Task<List<DoctorInvoiceSummaryDto>> GetDoctorInvoicesAsync(Guid doctorId)
        {
            return await _context.Database
                .SqlQueryRaw<DoctorInvoiceSummaryDto>("SELECT * FROM fn_get_invoices_by_doctor({0})", doctorId)
                .ToListAsync();
        }

        public async Task<List<BillingItemDto>> GetBillingItemsAsync()
        {
            return await _context.Database
                .SqlQueryRaw<BillingItemDto>("SELECT * FROM fn_get_billing_items()")
                .ToListAsync();
        }
        public async Task<List<PatientInvoiceSummaryDto>> GetPatientInvoicesAsync(Guid patientId)
        {
            return await _context.Database
                .SqlQueryRaw<PatientInvoiceSummaryDto>("SELECT * FROM fn_get_invoices_by_patient({0})", patientId)
                .ToListAsync();
        }
        public async Task<InvoiceDetailsDto?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            var rawData = await _context.Database
                .SqlQueryRaw<InvoiceDetailsSqlResult>("SELECT * FROM fn_get_invoice_details_by_id({0})", invoiceId)
                .FirstOrDefaultAsync();

            if (rawData == null) return null;

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var parsedItems = string.IsNullOrEmpty(rawData.ItemsJson)
                ? new List<InvoiceItemResponseDto>()
                : JsonSerializer.Deserialize<List<InvoiceItemResponseDto>>(rawData.ItemsJson, jsonOptions);

            var parsedPayments = string.IsNullOrEmpty(rawData.PaymentsJson)
                ? new List<InvoicePaymentResponseDto>()
                : JsonSerializer.Deserialize<List<InvoicePaymentResponseDto>>(rawData.PaymentsJson, jsonOptions);

            return new InvoiceDetailsDto
            {
                Id = rawData.Id,
                PatientId = rawData.PatientId,
                DoctorName = rawData.DoctorName,
                PatientName = rawData.PatientName,
                AppointmentId = rawData.AppointmentId,
                StartTime = rawData.StartTime ?? DateTime.MinValue,
                InvoiceDate = rawData.InvoiceDate,
                DoctorNotes = rawData.DoctorNotes ?? string.Empty,
                PatientAge = rawData.PatientAge,
                PatientGender = rawData.PatientGender,
                DoctorSpecialization = rawData.DoctorSpecialization,
                DoctorContactNumber = rawData.DoctorContactNumber,
                DoctorHospital = rawData.DoctorHospital,
                SubTotal = rawData.SubTotal,
                TotalTax = rawData.TotalTax,
                GrandTotal = rawData.GrandTotal,
                Items = parsedItems ?? new List<InvoiceItemResponseDto>(),
                Payments = parsedPayments ?? new List<InvoicePaymentResponseDto>()
            };
        }
        internal class InvoiceDetailsSqlResult
        {
            public Guid Id { get; set; }
            public Guid PatientId { get; set; }
            public string DoctorName { get; set; } = string.Empty;
            public string PatientName { get; set; } = string.Empty;
            public Guid? AppointmentId { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime InvoiceDate { get; set; }
            public string? DoctorNotes { get; set; }
            public string PatientAge { get; set; } = string.Empty;
            public string PatientGender { get; set; } = string.Empty;
            public string DoctorSpecialization { get; set; } = string.Empty;
            public string DoctorContactNumber { get; set; } = string.Empty;
            public string DoctorHospital { get; set; } = string.Empty;
            public decimal SubTotal { get; set; }
            public decimal TotalTax { get; set; }
            public decimal GrandTotal { get; set; }
            public string ItemsJson { get; set; } = string.Empty;
            public string PaymentsJson { get; set; } = string.Empty;
        }
    }
}