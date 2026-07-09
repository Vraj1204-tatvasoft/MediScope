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
using MediScope.Common.Models.Entities;

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
        public async Task<List<BillingItemDto>> GetBillingItemsAsync()
        {
            return await _context.Database
                .SqlQueryRaw<BillingItemDto>("SELECT * FROM fn_get_billing_items()")
                .ToListAsync();
        }
        public async Task<List<InvoiceSummaryDto>> GetInvoicesByUserIdAsync(Guid userId)
        {
            var doctorId = await _context.Doctors
                .Where(d => d.UserId == userId)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync();

            var patientId = await _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync();

            if (doctorId == null && patientId == null)
            {
                return new List<InvoiceSummaryDto>();
            }

            var pDoc = new NpgsqlParameter("@p_doctor_id", doctorId ?? (object)DBNull.Value);
            var pPat = new NpgsqlParameter("@p_patient_id", patientId ?? (object)DBNull.Value);

            return await _context.Database
                .SqlQueryRaw<InvoiceSummaryDto>("SELECT * FROM fn_get_invoices(@p_doctor_id, @p_patient_id)", pDoc, pPat)
                .ToListAsync();
        }
        public async Task IssueRefundAsync(List<Guid> refundIds, List<Guid> paymentIds, Guid invoiceId, string refundMode, string? reason, DateTime refundDate, decimal grandTotal, Guid? createdBy)
        {
            var pRefundIds = new NpgsqlParameter("@p_refund_ids", refundIds.ToArray()) { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid };
            var pPayIds = new NpgsqlParameter("@p_payment_ids", paymentIds.ToArray()) { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid };
            var pInvoice = new NpgsqlParameter("@p_invoice_id", invoiceId);
            var pMode = new NpgsqlParameter("@p_refund_mode", refundMode);
            var pReason = new NpgsqlParameter("@p_reason", reason ?? (object)DBNull.Value);
            var pDate = new NpgsqlParameter("@p_refund_date", refundDate);
            var pGrand = new NpgsqlParameter("@p_grand_total", grandTotal);
            var pCreatedBy = new NpgsqlParameter("@p_created_by", createdBy ?? (object)DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "CALL sp_issue_refund_bulk(@p_refund_ids, @p_payment_ids, @p_invoice_id, @p_refund_mode, @p_reason, @p_refund_date, @p_grand_total, @p_created_by)",
                pRefundIds, pPayIds, pInvoice, pMode, pReason, pDate, pGrand, pCreatedBy
            );
        }
        public async Task<List<Guid>> GetUnrefundedPaymentIdsAsync(Guid invoiceId)
        {
            return await _context.Payments
                .Include(p => p.Refund)
                .Where(p => p.InvoiceId == invoiceId && p.Refund == null && !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync();
        }
        public async Task<InvoiceDetailsDto?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            var rawData = await _context.Database
                .SqlQueryRaw<InvoiceDetailsSqlResult>("SELECT * FROM fn_get_invoice_details_by_id({0})", invoiceId)
                .FirstOrDefaultAsync();

            if (rawData == null) return null;

            var refunds = await _context.Database
                .SqlQueryRaw<InvoiceRefundResponseDto>("SELECT * FROM fn_get_refunds_by_invoice({0})", invoiceId)
                .ToListAsync();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var parsedItems = string.IsNullOrEmpty(rawData.ItemsJson)
                ? new List<InvoiceItemResponseDto>()
                : JsonSerializer.Deserialize<List<InvoiceItemResponseDto>>(rawData.ItemsJson, jsonOptions);

            var parsedPayments = string.IsNullOrEmpty(rawData.PaymentsJson)
                ? new List<InvoicePaymentResponseDto>()
                : JsonSerializer.Deserialize<List<InvoicePaymentResponseDto>>(rawData.PaymentsJson, jsonOptions);

            if (parsedPayments != null && refunds.Any())
            {
                foreach (var payment in parsedPayments)
                {
                    payment.Refunds = refunds.Where(r => r.PaymentId == payment.Id).ToList();
                }
            }

            return new InvoiceDetailsDto
            {
                Id = rawData.Id,
                PatientId = rawData.PatientId,
                DoctorName = rawData.DoctorName,
                PatientName = rawData.PatientName,
                PatientContact = rawData.PatientContact,
                PatientEmail = rawData.PatientEmail,
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
        public async Task<Guid> GetPatientIdByUserIdAsync(Guid userId)
        {
            return await _context.Patients
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();
        }

        public async Task SaveCardTokenAsync(
            Guid patientId, string tokenId, string last4, string network)
        {
            var existing = await _context.PatientCardTokens
                .Where(t => t.PatientId == patientId && t.IsActive && !t.IsDeleted)
                .ToListAsync();

            existing.ForEach(t =>
            {
                t.UpdatedAt = DateTime.UtcNow;
            });

            _context.PatientCardTokens.Add(new PatientCardToken
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                RazorpayTokenId = tokenId,
                Last4Digits = last4,
                CardNetwork = network,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
        public async Task<string?> GetRazorpayCustomerIdAsync(Guid patientId)
        {
            return await _context.Patients
                .Where(p => p.Id == patientId && !p.IsDeleted)
                .Select(p => p.RazorpayCustomerId)
                .FirstOrDefaultAsync();
        }

        public async Task SaveRazorpayCustomerIdAsync(Guid patientId, string customerId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient != null)
            {
                patient.RazorpayCustomerId = customerId;
                patient.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        internal class InvoiceDetailsSqlResult
        {
            public Guid Id { get; set; }
            public Guid PatientId { get; set; }
            public string DoctorName { get; set; } = string.Empty;
            public string PatientName { get; set; } = string.Empty;
            public string PatientContact { get; set; }
            public string PatientEmail { get; set; }
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