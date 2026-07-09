using System;
using System.Collections.Generic;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Payment : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }

        public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public Refund? Refund { get; set; }
    }
}