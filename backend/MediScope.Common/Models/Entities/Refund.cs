using System;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Refund : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public DateTime RefundDate { get; set; }
        public string RefundMode { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string? Reason { get; set; }
        public Payment Payment { get; set; } = null!;
    }
}