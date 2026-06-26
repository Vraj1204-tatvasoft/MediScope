using System;
using System.Collections.Generic;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid? BillingItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }

        public bool IsTax { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public BillingItem? BillingItem { get; set; }
        public Invoice Invoice { get; set; } = null!;
    }
}