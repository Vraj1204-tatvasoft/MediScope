namespace MediScope.Common.Models.DTOs.Request
{
    public class CreateRoomRequestDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public Guid WardId { get; set; }
        public Guid RoomTypeId { get; set; }
        public int NumberOfBeds { get; set; }
    }
}