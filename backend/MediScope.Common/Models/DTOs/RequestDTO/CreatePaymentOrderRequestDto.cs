namespace MediScope.Common.Models.DTOs.Request
{
    public class CreatePaymentOrderRequestDto
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
    }
}