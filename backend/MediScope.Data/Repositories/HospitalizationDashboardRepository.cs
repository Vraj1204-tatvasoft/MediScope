using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.DTOs.RequestDTO;
using MediScope.Common.Models.DTOs.ResponseDTO;
using MediScope.Data.Repositories;
using MediScope.Common.Models.Pagination;
using MediScope.Data;
namespace MediScope.Data.Repositories
{
    public class HospitalizationDashboardRepository : IHospitalizationDashboardRepository
    {
        private readonly AppDbContext _context;

        public HospitalizationDashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HospitalizationDashboardResponseDto> GetDashboardAsync(HospitalizationDashboardFilterDto request)
        {
            var summary = await GetSummaryAsync();
            var rooms = await GetRoomsAsync(request);

            return new HospitalizationDashboardResponseDto
            {
                Summary = summary,
                Rooms = rooms
            };
        }

        private async Task<HospitalizationSummaryDto> GetSummaryAsync()
        {
            const string sql = "SELECT * FROM fn_get_hospitalization_summary()";

            return await _context.Database
                .SqlQueryRaw<HospitalizationSummaryDto>(sql)
                .FirstAsync();
        }

        private async Task<PagedResult<HospitalizationRoomDto>> GetRoomsAsync(HospitalizationDashboardFilterDto request)
        {
            const string sql = @"SELECT * FROM fn_get_hospitalization_rooms(@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)";

            var items = await _context.Database.SqlQueryRaw<HospitalizationRoomDto>(
                sql,
                request.Search ?? (object)DBNull.Value,
                request.WardId ?? (object)DBNull.Value,
                request.RoomTypeId ?? (object)DBNull.Value,
                request.Floor ?? (object)DBNull.Value,
                request.OccupancyStatus ?? (object)DBNull.Value,
                request.SortBy ?? "roomnumber",
                request.SortDir ?? "asc",
                request.PageNumber,
                request.PageSize
            ).ToListAsync();

            return new PagedResult<HospitalizationRoomDto>
            {
                Items = items,
                TotalCount = (int)(items.FirstOrDefault()?.Total_Count ?? 0),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}