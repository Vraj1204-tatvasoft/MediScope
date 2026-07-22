using MediScope.Common.Models.Pagination;
namespace MediScope.Common.Models.DTOs.ResponseDTO
{
    public class HospitalizationDashboardResponseDto
    {
        public HospitalizationSummaryDto Summary { get; set; } = new();
        public PagedResult<HospitalizationRoomDto> Rooms { get; set; } = new();
    }
}