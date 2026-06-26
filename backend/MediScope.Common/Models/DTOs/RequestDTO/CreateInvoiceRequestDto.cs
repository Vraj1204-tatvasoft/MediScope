using System;
using System.Collections.Generic;
using MediScope.Common.Models.DTOs.Request;

namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateInvoiceRequestDto
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalPaid { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new();
    }
    public class InvoiceItemDto
    {
        public Guid? BillingItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public bool IsTax { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }
}