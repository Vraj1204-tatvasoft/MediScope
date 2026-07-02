
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequestDto dto)
        {
            try
            {
                if (dto == null || dto.Items.Count == 0)
                    return BadRequest("Invoice data and line items are required.");

                var invoiceId = await _invoiceService.CreateInvoiceAsync(dto);
                return CreatedAtAction(nameof(GetDoctorInvoices), new { doctorId = dto.DoctorId }, new { id = invoiceId });
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
            try
            {
                await _invoiceService.UpdateInvoiceAsync(id, dto);
                return NoContent(); // 204 No Content is standard for a successful PUT
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId}", id);
                return StatusCode(500, "An error occurred while updating the invoice.");
            }
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

        // GET: api/invoices/doctor/{doctorId}
        [HttpGet("doctor/{doctorId:guid}")]
        public async Task<ActionResult<List<DoctorInvoiceSummaryDto>>> GetDoctorInvoices(Guid doctorId)
        {
            try
            {
                var invoices = await _invoiceService.GetDoctorInvoicesAsync(doctorId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoices for doctor {DoctorId}", doctorId);
                return StatusCode(500, "An error occurred while retrieving the invoices.");
            }
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
    }
}