using MediScope.Common.Models.Enums;
using MediScope.Common.Models.Pagination;
namespace MediScope.Common.Models.DTOs.RequestDTO
{
    public class HospitalizationDashboardFilterDto : PaginationParams
    {
        public Guid? WardId { get; set; }
        public Guid? RoomTypeId { get; set; }
        public int? Floor { get; set; }
        public OccupancyStatus? OccupancyStatus { get; set; }
    }
}