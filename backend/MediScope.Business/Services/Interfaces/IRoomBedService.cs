using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;

namespace MediScope.Business.Services.Interfaces
{
    public interface IRoomBedService
    {
        Task<bool> CreateRoomAsync(CreateRoomRequestDto request);
        Task<bool> CreateWardAsync(CreateWardRequestDto request);
        Task<bool> UpdateWardAsync(Guid id, UpdateWardRequestDto request);
        Task<bool> DeleteWardAsync(Guid id);
        Task<bool> UpdateRoomAsync(Guid id, UpdateRoomRequestDto request);
        Task<bool> DeleteRoomAsync(Guid id);
        Task<bool> DeleteBedAsync(Guid id);
        Task<bool> UpdateBedAsync(Guid id, UpdateBedRequestDto request);
        Task<bool> CreateRoomTypeAsync(CreateRoomTypeDto request);
        Task<bool> UpdateRoomTypeAsync(Guid id, UpdateRoomTypeDto request);
        Task<bool> DeleteRoomTypeAsync(Guid id);
        Task<BedSummaryDto> GetBedByIdAsync(Guid id);
        Task<PagedResult<WardSummaryResponseDto>> GetWardsPagedAsync(PaginationParams request);
        Task<PagedResult<RoomTypeDto>> GetRoomTypesPagedAsync(PaginationParams request);
        Task<PagedResult<RoomSummaryResponseDto>> GetRoomsPagedAsync(PaginationParams request);
        Task<PagedResult<BedSummaryDto>> GetBedsPagedAsync(PaginationParams request);
    }
}