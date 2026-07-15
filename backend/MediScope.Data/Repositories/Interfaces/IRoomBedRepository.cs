using MediScope.Common.Models.Entities;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Pagination;
namespace MediScope.Data.Repositories
{
    public interface IRoomBedRepository
    {
        Task CreateRoomWithBedsAsync(string roomNumber, Guid wardId, Guid roomTypeId, int numberOfBeds);
        Task CreateWardAsync(string name, string? description);
        Task UpdateWardAsync(Guid id, string name, string? description);
        Task DeleteWardAsync(Guid id);
        Task UpdateRoomAsync(Guid id, string roomNumber, Guid wardId, Guid roomTypeId);
        Task DeleteRoomAsync(Guid id);
        Task DeleteBedAsync(Guid id);
        Task UpdateBedAsync(Guid id, string bedNumber, int status);
        Task CreateRoomTypeAsync(string name);
        Task UpdateRoomTypeAsync(Guid id, string name);
        Task DeleteRoomTypeAsync(Guid id);
        Task<BedSummaryDto?> GetBedByIdAsync(Guid id);
        Task<PagedResult<WardSummaryResponseDto>> GetWardsPagedAsync(PaginationParams request);
        Task<PagedResult<RoomTypeDto>> GetRoomTypesPagedAsync(PaginationParams request);
        Task<PagedResult<RoomSummaryResponseDto>> GetRoomsPagedAsync(PaginationParams request);
        Task<PagedResult<BedSummaryDto>> GetBedsPagedAsync(PaginationParams request);
    }
}