
namespace MediScope.Common.Models.Entities
{
    public class RoomType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}