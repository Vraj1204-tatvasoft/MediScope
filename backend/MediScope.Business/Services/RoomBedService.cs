using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Pagination;
using MediScope.Common.Models.Enums;

namespace MediScope.Business.Services
{
    public class RoomBedService : IRoomBedService
    {
        private readonly IRoomBedRepository _repository;
        private readonly IHubContext<RealtimeHub> _hubContext;
        public RoomBedService(IRoomBedRepository repository, IHubContext<RealtimeHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        public async Task<bool> CreateRoomAsync(CreateRoomRequestDto request)
        {
            if (request.NumberOfBeds <= 0 || request.NumberOfBeds > 50)
            {
                throw new ArgumentException("Number of beds must be between 1 and 50.");
            }

            await _repository.CreateRoomWithBedsAsync(
                request.RoomNumber,
                request.Floor,
                request.WardId,
                request.RoomTypeId,
                request.NumberOfBeds);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }
        public async Task<bool> CreateWardAsync(CreateWardRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Ward name cannot be empty.");
            }

            await _repository.CreateWardAsync(request.Name, request.Description);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<PagedResult<WardSummaryResponseDto>> GetWardsPagedAsync(PaginationParams request)
             => await _repository.GetWardsPagedAsync(request);

        public async Task<PagedResult<RoomTypeDto>> GetRoomTypesPagedAsync(PaginationParams request)
            => await _repository.GetRoomTypesPagedAsync(request);

        public async Task<PagedResult<RoomSummaryResponseDto>> GetRoomsPagedAsync(PaginationParams request)
            => await _repository.GetRoomsPagedAsync(request);

        public async Task<PagedResult<BedSummaryDto>> GetBedsPagedAsync(PaginationParams request)
            => await _repository.GetBedsPagedAsync(request);

        public async Task<bool> UpdateWardAsync(Guid id, UpdateWardRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Ward name cannot be empty.");

            await _repository.UpdateWardAsync(id, request.Name, request.Description);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> DeleteWardAsync(Guid id)
        {
            await _repository.DeleteWardAsync(id);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> UpdateRoomAsync(Guid id, UpdateRoomRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RoomNumber))
                throw new ArgumentException("Room number cannot be empty.");

            await _repository.UpdateRoomAsync(id, request.RoomNumber, request.Floor, request.WardId, request.RoomTypeId);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> DeleteRoomAsync(Guid id)
        {
            await _repository.DeleteRoomAsync(id);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> DeleteBedAsync(Guid id)
        {
            await _repository.DeleteBedAsync(id);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> UpdateBedAsync(Guid id, UpdateBedRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.BedNumber))
            {
                throw new ArgumentException("Bed number cannot be empty.");
            }
            await _repository.UpdateBedAsync(id, request.BedNumber, (int)request.Status);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }
        public async Task<bool> CreateRoomTypeAsync(CreateRoomTypeDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Room type name cannot be empty.");

            await _repository.CreateRoomTypeAsync(request.Name);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> UpdateRoomTypeAsync(Guid id, UpdateRoomTypeDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Room type name cannot be empty.");

            await _repository.UpdateRoomTypeAsync(id, request.Name);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<bool> DeleteRoomTypeAsync(Guid id)
        {
            await _repository.DeleteRoomTypeAsync(id);
            await _hubContext.Clients.All.SendAsync("DashboardUpdated");
            return true;
        }

        public async Task<BedSummaryDto> GetBedByIdAsync(Guid id)
        {
            var bed = await _repository.GetBedByIdAsync(id);
            if (bed == null)
            {
                throw new KeyNotFoundException($"Bed with ID {id} was not found.");
            }
            return bed;
        }
    }
}