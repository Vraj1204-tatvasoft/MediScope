using System;

namespace MediScope.Common.Models.DTOs.Response
{
    public class BillingItemDto
    {
        public Guid Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultAmount { get; set; }
        public bool IsTaxable { get; set; }
    }
}