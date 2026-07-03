using System;

namespace MediScope.Common.Models.DTOs.Request
{
    public class IssueRefundRequestDto
    {
        public List<Guid>? PaymentIds { get; set; }
        public Guid InvoiceId { get; set; }
        public string RefundMode { get; set; } = string.Empty;
        public DateTime RefundDate { get; set; }
        public string? Reason { get; set; }
        public decimal GrandTotal { get; set; }
    }
}