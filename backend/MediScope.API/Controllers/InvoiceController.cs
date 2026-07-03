
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
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
        {
            _invoiceService = invoiceService;
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
    }
}