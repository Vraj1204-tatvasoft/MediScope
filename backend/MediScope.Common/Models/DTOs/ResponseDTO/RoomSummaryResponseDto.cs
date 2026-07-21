namespace MediScope.Common.Models.DTOs.Response
{
    public class RoomSummaryResponseDto
    {
        public Guid Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public int BedCount { get; set; }
        public int AvailableBeds { get; set; }
        public int Floor { get; set; }
        public Guid Ward_Id { get; set; }
        public Guid Room_Type_Id { get; set; }
        public long Total_Count { get; set; }
    }
}