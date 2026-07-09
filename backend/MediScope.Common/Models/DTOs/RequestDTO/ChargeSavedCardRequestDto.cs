using System.ComponentModel.DataAnnotations;

namespace MediScope.Common.Models.DTOs.Request
{
    public class ChargeSavedCardRequestDto
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string TokenId { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
    }
}