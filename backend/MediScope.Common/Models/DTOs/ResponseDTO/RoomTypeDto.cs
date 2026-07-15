namespace MediScope.Common.Models.DTOs.Response
{
    public class RoomTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Total_Count { get; set; }
    }
}