
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoicesController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IRazorpayService _razorpayService;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(IInvoiceService invoiceService, IRazorpayService razorpayService, ILogger<InvoicesController> logger)
        {
            _invoiceService = invoiceService;
            _razorpayService = razorpayService;
            _logger = logger;
        }

        // POST: api/invoices
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto dto)
        {
            try
            {
                if (dto == null || dto.Items.Count == 0)
                    return BadRequest("Invoice data and line items are required.");

                var invoiceId = await _invoiceService.CreateInvoiceAsync(dto);
                return CreatedAtAction(nameof(GetInvoiceById), new { id = invoiceId }, new { id = invoiceId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, "An error occurred while creating the invoice.");
            }
        }

        // PUT: api/invoices/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] CreateInvoiceRequestDto dto)
        {
            await _invoiceService.UpdateInvoiceAsync(id, dto);
            return NoContent();
        }

        // DELETE: api/invoices/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            try
            {
                await _invoiceService.DeleteInvoiceAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);

                if (ex.InnerException?.Message.Contains("Cannot delete an invoice that has registered payments") == true ||
                    ex.Message.Contains("Cannot delete an invoice that has registered payments"))
                {
                    return BadRequest("Cannot delete this invoice because partial or full payments have already been recorded.");
                }

                return StatusCode(500, "An error occurred while deleting the invoice.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyInvoices()
        {
            var invoices = await _invoiceService.GetMyInvoicesAsync(CurrentUserId);
            return Success(invoices);
        }

        [HttpGet("billing-items")]
        public async Task<ActionResult<List<BillingItemDto>>> GetBillingItems()
        {
            try
            {
                var items = await _invoiceService.GetBillingItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billing items");
                return StatusCode(500, "An error occurred while retrieving billing items.");
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetInvoiceById(Guid id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null) return NotFound("Invoice not found.");
            return Ok(invoice);
        }

        // POST: api/invoices/{id}/refund
        [HttpPost("{id:guid}/refund")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> IssueRefund(Guid id, [FromBody] IssueRefundRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Refund data is required.");

            if (id != dto.InvoiceId)
                return BadRequest("The Invoice ID in the URL does not match the payload.");

            await _invoiceService.IssueRefundAsync(dto, CurrentUserId);

            return Ok(new { success = true, message = "Refund processed successfully." });
        }
        [HttpPost("{id:guid}/payment/order")]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> CreatePaymentOrder(
            Guid id, [FromBody] CreatePaymentOrderRequestDto dto)
        {
            try
            {
                if (dto.PaymentMode.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                {
                    var cashInvoice = await _invoiceService.GetInvoiceByIdAsync(id);
                    if (cashInvoice == null) return NotFoundResponse("Invoice not found.");

                    var cashUpdateDto = BuildUpdateDto(cashInvoice, dto.Amount, dto.PaymentMode);
                    await _invoiceService.UpdateInvoiceAsync(id, cashUpdateDto);
                    return Success<object>(null!, "Cash payment recorded successfully.");
                }

                // Get or create Razorpay customer for this patient
                // Get or create Razorpay customer for this patient
                var patientId = await _invoiceService.GetPatientIdByUserIdAsync(CurrentUserId);
                string? customerId = null;
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (patientId != Guid.Empty)
                {
                    // GetOrCreateCustomer — creates in Razorpay AND saves to DB if first time
                    // previously you were only reading, never creating

                    customerId = await _razorpayService.GetOrCreateCustomerAsync(
                        patientId,
                        invoice?.PatientName ?? "Patient",
                        invoice?.PatientEmail,
                        invoice?.PatientContact
                    );
                }

                var order = await _razorpayService.CreateOrderAsync(id, dto.Amount, customerId, invoice.PatientContact, invoice.PatientEmail);
                return Success(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment order for invoice {InvoiceId}", id);
                return ServerError("An error occurred while creating the payment order.");
            }
        }

        // POST: api/invoices/{id}/payment/verify
        // Step 2 — frontend calls this after Razorpay checkout completes
        [HttpPost("{id:guid}/payment/verify")]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> VerifyAndRecordPayment(
            Guid id, [FromBody] VerifyPaymentRequestDto dto)
        {
            _logger.LogInformation(
                "Verify called — OrderId: {OrderId}, PaymentId: {PaymentId}, Signature: {Sig}",
                dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature);
            // Always verify signature before trusting anything from frontend
            if (!_razorpayService.VerifySignature(
                    dto.RazorpayOrderId,
                    dto.RazorpayPaymentId,
                    dto.RazorpaySignature))
            {
                _logger.LogWarning(
                    "Invalid Razorpay signature for invoice {InvoiceId}", id);
                return BadRequestResponse("Payment verification failed. Invalid signature.");
            }

            try
            {
                // Fetch current invoice to preserve items and totals
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                    return NotFoundResponse("Invoice not found.");

                // Pass payment to SP via existing UpdateInvoiceAsync —
                // SP handles insert, total_paid recalc and status update
                var updateDto = BuildUpdateDto(invoice, dto.Amount, dto.PaymentMode, dto.RazorpayPaymentId);
                await _invoiceService.UpdateInvoiceAsync(id, updateDto);

                // Save card token only if patient opted in
                if (dto.SaveCard && IsCardMode(dto.PaymentMode))
                {
                    var patientId = await _invoiceService.GetPatientIdByUserIdAsync(CurrentUserId);
                    if (patientId != Guid.Empty)
                    {
                        await _razorpayService.FetchAndSaveCardTokenAsync(
                            dto.RazorpayPaymentId, patientId);
                    }
                }

                return Success<object>(null!, "Payment recorded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment for invoice {InvoiceId}", id);
                return ServerError("An error occurred while recording the payment.");
            }
        }
        [HttpGet("payment/tokens")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetCustomerTokens()
        {
            try
            {
                var patientId = await _invoiceService.GetPatientIdByUserIdAsync(CurrentUserId);
                var customerId = await _invoiceService.GetRazorpayCustomerIdAsync(patientId);

                if (string.IsNullOrEmpty(customerId))
                    return NotFoundResponse("No Razorpay customer found for this patient.");

                var tokens = await _razorpayService.GetCustomerTokensAsync(customerId);
                return Success(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tokens");
                return ServerError(ex.Message);
            }
        }
        // POST: api/invoices/{id}/payment/saved-card
        /*[HttpPost("{id:guid}/payment/saved-card")]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> ChargeSavedCard(
            Guid id, [FromBody] ChargeSavedCardRequestDto dto)
        {
            try
            {
                var patientId = await _invoiceService.GetPatientIdByUserIdAsync(CurrentUserId);
                var customerId = await _invoiceService.GetRazorpayCustomerIdAsync(patientId);

                if (string.IsNullOrEmpty(customerId))
                    return BadRequestResponse("No Razorpay customer found.");

                var paymentId = await _razorpayService.ChargeSavedCardAsync(
                    dto.TokenId, customerId, id, dto.Amount);

                // Record payment in DB
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null) return NotFoundResponse("Invoice not found.");

                var updateDto = BuildUpdateDto(invoice, dto.Amount, dto.PaymentMode, paymentId);
                await _invoiceService.UpdateInvoiceAsync(id, updateDto);

                return Success<object>(null!, "Payment recorded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error charging saved card for invoice {InvoiceId}", id);
                return ServerError("An error occurred while processing the saved card payment.");
            }
        }*/
        private static CreateInvoiceRequestDto BuildUpdateDto(
        InvoiceDetailsDto invoice, decimal amount, string paymentMode, string? razorpayPaymentId = null)
        {
            return new CreateInvoiceRequestDto
            {
                PatientId = invoice.PatientId,
                AppointmentId = invoice.AppointmentId.GetValueOrDefault(),
                SubTotal = invoice.SubTotal,
                TotalDiscount = 0,
                TotalTax = invoice.TotalTax,
                GrandTotal = invoice.GrandTotal,
                Items = invoice.Items.Select(i => new InvoiceItemDto
                {
                    BillingItemId = i.BillingItemId,
                    Description = i.Description,
                    Amount = i.Amount,
                    Discount = 0,       // not on response DTO, default to 0
                    IsTax = i.IsTax,
                    Tax = 0,       // not on response DTO, default to 0
                    Total = i.Amount // not on response DTO, use Amount as fallback
                }).ToList(),
                Payments = new List<PaymentDto>
                {
                    new PaymentDto
                    {
                        PaymentDate   = DateTime.UtcNow,
                        PaymentMode   = paymentMode,
                        PaymentAmount = amount,
                        RazorpayPaymentId = razorpayPaymentId
                    }
                }
            };
        }

        private static bool IsCardMode(string mode) =>
            mode.Equals("Credit Card", StringComparison.OrdinalIgnoreCase) ||
            mode.Equals("Debit Card", StringComparison.OrdinalIgnoreCase);
    }
}