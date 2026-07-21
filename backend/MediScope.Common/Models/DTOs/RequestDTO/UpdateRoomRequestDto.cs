namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateRoomRequestDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public Guid WardId { get; set; }
        public Guid RoomTypeId { get; set; }
    }
}