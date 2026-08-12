using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Razorpay.Api;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly string _currency;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ILogger<RazorpayService> _logger;

        public RazorpayService(
            IConfiguration config,
            IInvoiceRepository invoiceRepository,
            ILogger<RazorpayService> logger)
        {
            _keyId = config["Razorpay:KeyId"]!;
            _keySecret = config["Razorpay:KeySecret"]!;
            _currency = config["Razorpay:Currency"] ?? "USD";
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }
        public async Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid invoiceId, decimal amount, string razorpayCustomerId, string? patientContact, string? patientEmail)
        {
            var client = new RazorpayClient(_keyId, _keySecret);

            var options = new Dictionary<string, object>
            {
                { "amount",          (long)(amount * 100) },
                { "currency",        _currency },
                { "receipt",         $"inv_{invoiceId}" },
                { "payment_capture", 1 }
            };
            if (!string.IsNullOrEmpty(razorpayCustomerId))
            {
                options.Add("customer_id", razorpayCustomerId);
            }
            var order = await Task.Run(() => client.Order.Create(options));

            return new CreatePaymentOrderResponseDto
            {
                OrderId = order["id"].ToString()!,
                KeyId = _keyId,
                Amount = amount,
                Currency = _currency,
                RazorpayCustomerId = razorpayCustomerId,
                PatientContact = patientContact,
                PatientEmail = patientEmail
            };
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            // Razorpay: HMAC-SHA256(orderId + "|" + paymentId, KeySecret)
            var payload = $"{orderId}|{paymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var generated = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return generated == signature;
        }

        public async Task<string?> FetchAndSaveCardTokenAsync(string razorpayPaymentId, Guid patientId)
        {
            try
            {
                var client = new RazorpayClient(_keyId, _keySecret);
                var payment = await Task.Run(() => client.Payment.Fetch(razorpayPaymentId));

                var tokenId = payment["token_id"]?.ToString();
                if (string.IsNullOrEmpty(tokenId)) return null;

                // Extract last 4 and network for display — never store full card number
                var last4 = payment["card"]?["last4"]?.ToString() ?? "****";
                var network = payment["card"]?["network"]?.ToString() ?? "Unknown";

                await _invoiceRepository.SaveCardTokenAsync(patientId, tokenId, last4, network);
                return tokenId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save card token for patient {PatientId}", patientId);
                return null;
            }
        }
        public async Task<string> GetOrCreateCustomerAsync(
            Guid patientId, string patientName, string? email, string? contact)
        {
            _logger.LogInformation("GetOrCreateCustomer called for patient {PatientId}", patientId);
            var existingId = await _invoiceRepository.GetRazorpayCustomerIdAsync(patientId);
            _logger.LogInformation("Existing Razorpay customer ID: {CustomerId}", existingId ?? "NULL");

            if (!string.IsNullOrEmpty(existingId))
                return existingId;

            var client = new RazorpayClient(_keyId, _keySecret);
            var options = new Dictionary<string, object>
            {
                { "name",          patientName },
                { "email",         email    ?? $"patient_{patientId}@mediscope.com" },
                { "contact",       contact  ?? "" },
                { "fail_existing", 0 }
            };

            var customer = await Task.Run(() => client.Customer.Create(options));
            var customerId = customer["id"].ToString()!;

            await _invoiceRepository.SaveRazorpayCustomerIdAsync(patientId, customerId);
            return customerId;
        }
        public async Task<string> GetCustomerTokensAsync(string customerId)
        {
            using var httpClient = new HttpClient();

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var response = await httpClient.GetAsync(
                $"https://api.razorpay.com/v1/customers/{customerId}/tokens");

            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Tokens response: {Content}", content);

            return content;
        }
        /*public async Task<string> ChargeSavedCardAsync(string tokenId, string customerId, Guid invoiceId, decimal amount)
        {
            using var httpClient = new HttpClient();
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            // Step 1: Create order
            var orderPayload = new
            {
                amount = (long)(amount * 100),
                currency = _currency,
                receipt = $"inv_{invoiceId}",
                payment_capture = 1
            };

            var orderResponse = await httpClient.PostAsync(
                "https://api.razorpay.com/v1/orders",
                new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(orderPayload),
                    Encoding.UTF8, "application/json"));

            var orderContent = await orderResponse.Content.ReadAsStringAsync();
            var orderJson = System.Text.Json.JsonDocument.Parse(orderContent);
            var orderId = orderJson.RootElement.GetProperty("id").GetString()!;

            // Step 2: Charge token directly — no frontend needed
            var paymentPayload = new
            {
                amount = (long)(amount * 100),
                currency = _currency,
                order_id = orderId,
                customer_id = customerId,
                token = tokenId,
                recurring = "1",
                description = $"Invoice {invoiceId}",
                notes = new { invoice_id = invoiceId.ToString() }
            };

            var paymentResponse = await httpClient.PostAsync(
                "https://api.razorpay.com/v1/payments/create/recurring",
                new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(paymentPayload),
                    Encoding.UTF8, "application/json"));

            var paymentContent = await paymentResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Saved card charge response: {Content}", paymentContent);

            var paymentJson = System.Text.Json.JsonDocument.Parse(paymentContent);
            return paymentJson.RootElement.GetProperty("razorpay_payment_id").GetString()!;
        }*/
    }
}