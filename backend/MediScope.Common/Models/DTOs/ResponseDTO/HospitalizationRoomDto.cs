namespace MediScope.Common.Models.DTOs.ResponseDTO
{
    public class HospitalizationRoomDto
    {
        public Guid Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public int Floor { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public string OccupancyStatus { get; set; } = string.Empty;
        public Guid Ward_Id { get; set; }
        public Guid Room_Type_Id { get; set; }
        public long Total_Count { get; set; }
    }
}