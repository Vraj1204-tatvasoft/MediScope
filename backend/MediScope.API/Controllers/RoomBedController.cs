using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Pagination;

namespace MediScope.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class RoomBedController : BaseController
    {
        private readonly IRoomBedService _roomBedService;

        public RoomBedController(IRoomBedService roomBedService)
        {
            _roomBedService = roomBedService;
        }


        [HttpPost("rooms")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequestDto request)
        {
            await _roomBedService.CreateRoomAsync(request);
            return NoContent("Room and beds generated successfully.");
        }

        [HttpPost("wards")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateWard([FromBody] CreateWardRequestDto request)
        {
            await _roomBedService.CreateWardAsync(request);
            return NoContent("Ward created successfully.");
        }

        [HttpPut("wards/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateWard(Guid id, [FromBody] UpdateWardRequestDto request)
        {
            await _roomBedService.UpdateWardAsync(id, request);
            return NoContent("Ward updated successfully.");
        }

        [HttpDelete("wards/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteWard(Guid id)
        {
            await _roomBedService.DeleteWardAsync(id);
            return NoContent("Ward and associated rooms/beds deleted successfully.");
        }

        [HttpPut("rooms/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomRequestDto request)
        {
            await _roomBedService.UpdateRoomAsync(id, request);
            return NoContent("Room updated successfully.");
        }

        [HttpDelete("rooms/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            await _roomBedService.DeleteRoomAsync(id);
            return NoContent("Room and associated beds deleted successfully.");
        }

        [HttpDelete("beds/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteBed(Guid id)
        {
            await _roomBedService.DeleteBedAsync(id);
            return NoContent("Bed deleted successfully.");
        }

        [HttpPut("beds/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateBed(Guid id, [FromBody] UpdateBedRequestDto request)
        {
            await _roomBedService.UpdateBedAsync(id, request);
            return NoContent("Bed updated successfully.");
        }

        [HttpPost("room-types")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeDto request)
        {
            await _roomBedService.CreateRoomTypeAsync(request);
            return NoContent("Room type created successfully.");
        }

        [HttpPut("room-types/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateRoomType(Guid id, [FromBody] UpdateRoomTypeDto request)
        {
            await _roomBedService.UpdateRoomTypeAsync(id, request);
            return NoContent("Room type updated successfully.");
        }

        [HttpDelete("room-types/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteRoomType(Guid id)
        {
            await _roomBedService.DeleteRoomTypeAsync(id);
            return NoContent("Room type deleted successfully.");
        }

        [HttpGet("beds/{id:guid}")]
        [Authorize(Policy = "PatientOrAdmin")]
        public async Task<IActionResult> GetBedById(Guid id)
        {
            var bed = await _roomBedService.GetBedByIdAsync(id);
            return Success(bed);
        }

        [HttpGet("rooms")]
        [Authorize(Policy = "PatientOrAdmin")]
        public async Task<IActionResult> GetRooms([FromQuery] PaginationParams request)
        {
            var pagedResult = await _roomBedService.GetRoomsPagedAsync(request);
            return Success(pagedResult);
        }

        [HttpGet("wards")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetWards([FromQuery] PaginationParams request)
        {
            var pagedResult = await _roomBedService.GetWardsPagedAsync(request);
            return Success(pagedResult);
        }

        [HttpGet("room-types")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetRoomTypes([FromQuery] PaginationParams request)
        {
            var pagedResult = await _roomBedService.GetRoomTypesPagedAsync(request);
            return Success(pagedResult);
        }

        [HttpGet("beds")]
        [Authorize(Policy = "PatientOrAdmin")]
        public async Task<IActionResult> GetAllBeds([FromQuery] PaginationParams request)
        {
            var pagedResult = await _roomBedService.GetBedsPagedAsync(request);
            return Success(pagedResult);
        }
    }
}