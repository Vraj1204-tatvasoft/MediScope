using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Response
{
    public class BedSummaryDto
    {
        public Guid Id { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public BedStatus Status { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public long Total_Count { get; set; }
    }
}