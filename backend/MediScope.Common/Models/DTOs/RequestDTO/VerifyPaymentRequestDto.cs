namespace MediScope.Common.Models.DTOs.Request
{
    public class VerifyPaymentRequestDto
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
        public bool SaveCard { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}