using System;

namespace MediScope.Common.Models.Entities
{
    public class BillingItem
    {
        public Guid Id { get; set; }

        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal DefaultAmount { get; set; }
        public bool IsTaxable { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}