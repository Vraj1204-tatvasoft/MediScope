using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Business.Services.Interfaces;

namespace MediScope.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class BroadcastsController : BaseController
    {
        private readonly IBroadcastService _broadcastService;

        public BroadcastsController(IBroadcastService broadcastService)
        {
            _broadcastService = broadcastService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] GetBroadcastsRequestDto request)
        {
            var result = await _broadcastService.GetBroadcastsPagedAsync(request);
            return Success(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _broadcastService.GetBroadcastByIdAsync(id);

            if (result == null)
            {
                return NotFoundResponse($"Broadcast with ID {id} was not found.");
            }

            return Success(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBroadcastRequestDto request)
        {
            var newId = await _broadcastService.CreateBroadcastAsync(request, CurrentUserId);

            return Created(new { Id = newId }, "Broadcast created successfully.");
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBroadcastRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestResponse("Invalid data.");
            }
            await _broadcastService.UpdateBroadcastAsync(id, request);

            return NoContent("Broadcast updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _broadcastService.SoftDeleteBroadcastAsync(id);

            return NoContent("Broadcast deleted successfully.");
        }
        [HttpPost("{id:guid}/send")]
        public async Task<IActionResult> Send(Guid id)
        {
            int totalRecipients = await _broadcastService.SendBroadcastAsync(id);

            return Success(new { Message = "Broadcast queued for delivery.", TotalRecipients = totalRecipients });
        }
        [HttpPost("{id:guid}/retry")]
        public async Task<IActionResult> Retry(Guid id)
        {
            int failedCount = await _broadcastService.RetryBroadcastAsync(id);

            return Success(new { Message = "Retry queued for failed recipients.", FailedCount = failedCount });
        }
    }
}