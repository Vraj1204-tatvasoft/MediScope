namespace MediScope.Common.Models.DTOs.Response
{
    public class CreatePaymentOrderResponseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? RazorpayCustomerId { get; set; }
        public string? PatientContact { get; set; }
        public string? PatientEmail { get; set; }
    }
}