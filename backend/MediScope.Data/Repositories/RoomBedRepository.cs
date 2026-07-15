using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;
using MediScope.Data;
namespace MediScope.Data.Repositories
{
    public class RoomBedRepository : IRoomBedRepository
    {
        private readonly AppDbContext _context;

        public RoomBedRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateRoomWithBedsAsync(string roomNumber, Guid wardId, Guid roomTypeId, int numberOfBeds)
        {
            var sql = "CALL sp_create_room_with_beds(@p0, @p1, @p2, @p3)";

            await _context.Database.ExecuteSqlRawAsync(sql,
                roomNumber,
                wardId,
                roomTypeId,
                numberOfBeds);
        }


        public async Task CreateWardAsync(string name, string? description)
        {
            var sql = "CALL sp_create_ward(@p0, @p1)";
            await _context.Database.ExecuteSqlRawAsync(sql, name, description ?? (object)DBNull.Value);
        }
        public async Task UpdateWardAsync(Guid id, string name, string? description)
        {
            var sql = "CALL sp_update_ward(@p0, @p1, @p2)";
            await _context.Database.ExecuteSqlRawAsync(sql, id, name, description ?? (object)DBNull.Value);
        }

        public async Task DeleteWardAsync(Guid id)
        {
            var sql = "CALL sp_soft_delete_ward(@p0)";
            await _context.Database.ExecuteSqlRawAsync(sql, id);
        }

        public async Task UpdateRoomAsync(Guid id, string roomNumber, Guid wardId, Guid roomTypeId)
        {
            var sql = "CALL sp_update_room(@p0, @p1, @p2, @p3)";
            await _context.Database.ExecuteSqlRawAsync(sql, id, roomNumber, wardId, roomTypeId);
        }

        public async Task DeleteRoomAsync(Guid id)
        {
            var sql = "CALL sp_soft_delete_room(@p0)";
            await _context.Database.ExecuteSqlRawAsync(sql, id);
        }

        public async Task DeleteBedAsync(Guid id)
        {
            var sql = "CALL sp_soft_delete_bed(@p0)";
            await _context.Database.ExecuteSqlRawAsync(sql, id);
        }
        public async Task UpdateBedAsync(Guid id, string bedNumber, int status)
        {
            var sql = "CALL sp_update_bed(@p0, @p1, @p2)";
            await _context.Database.ExecuteSqlRawAsync(sql, id, bedNumber, status);
        }
        public async Task CreateRoomTypeAsync(string name)
        {
            var sql = "CALL sp_create_room_type(@p0)";
            await _context.Database.ExecuteSqlRawAsync(sql, name);
        }

        public async Task UpdateRoomTypeAsync(Guid id, string name)
        {
            var sql = "CALL sp_update_room_type(@p0, @p1)";
            await _context.Database.ExecuteSqlRawAsync(sql, id, name);
        }

        public async Task DeleteRoomTypeAsync(Guid id)
        {
            var sql = "CALL sp_soft_delete_room_type(@p0)";
            await _context.Database.ExecuteSqlRawAsync(sql, id);
        }

        public async Task<BedSummaryDto?> GetBedByIdAsync(Guid id)
        {
            var sql = $"SELECT * FROM fn_get_bed_by_id('{id}'::uuid)";

            return await _context.Database
                .SqlQueryRaw<BedSummaryDto>(sql)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<WardSummaryResponseDto>> GetWardsPagedAsync(PaginationParams request)
        {
            var sql = "SELECT * FROM fn_get_wards_paged(@p0, @p1, @p2, @p3, @p4)";
            var items = await _context.Database.SqlQueryRaw<WardSummaryResponseDto>(
                sql,
                request.Search ?? (object)DBNull.Value,
                request.SortBy ?? "name",
                request.SortDir ?? "asc",
                request.PageNumber,
                request.PageSize
            ).ToListAsync();

            return new PagedResult<WardSummaryResponseDto>
            {
                Items = items,
                TotalCount = (int)(items.FirstOrDefault()?.Total_Count ?? 0),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<RoomTypeDto>> GetRoomTypesPagedAsync(PaginationParams request)
        {
            var sql = "SELECT * FROM fn_get_room_types_paged(@p0, @p1, @p2, @p3, @p4)";
            var items = await _context.Database.SqlQueryRaw<RoomTypeDto>(
                sql,
                request.Search ?? (object)DBNull.Value,
                request.SortBy ?? "name",
                request.SortDir ?? "asc",
                request.PageNumber,
                request.PageSize
            ).ToListAsync();

            return new PagedResult<RoomTypeDto>
            {
                Items = items,
                TotalCount = (int)(items.FirstOrDefault()?.Total_Count ?? 0),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<RoomSummaryResponseDto>> GetRoomsPagedAsync(PaginationParams request)
        {
            var sql = "SELECT * FROM fn_get_rooms_summary_paged(@p0, @p1, @p2, @p3, @p4)";
            var items = await _context.Database.SqlQueryRaw<RoomSummaryResponseDto>(
                sql,
                request.Search ?? (object)DBNull.Value,
                request.SortBy ?? "roomnumber",
                request.SortDir ?? "asc",
                request.PageNumber,
                request.PageSize
            ).ToListAsync();

            return new PagedResult<RoomSummaryResponseDto>
            {
                Items = items,
                TotalCount = (int)(items.FirstOrDefault()?.Total_Count ?? 0),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<BedSummaryDto>> GetBedsPagedAsync(PaginationParams request)
        {
            var sql = "SELECT * FROM fn_get_beds_paged(@p0, @p1, @p2, @p3, @p4)";
            var items = await _context.Database.SqlQueryRaw<BedSummaryDto>(
                sql,
                request.Search ?? (object)DBNull.Value,
                request.SortBy ?? "bednumber",
                request.SortDir ?? "asc",
                request.PageNumber,
                request.PageSize
            ).ToListAsync();

            return new PagedResult<BedSummaryDto>
            {
                Items = items,
                TotalCount = (int)(items.FirstOrDefault()?.Total_Count ?? 0),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}