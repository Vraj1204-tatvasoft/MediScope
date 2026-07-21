
namespace MediScope.Common.Models.Entities
{
    public class Room : BaseEntity
    {
        public string RoomNumber { get; set; } = string.Empty;
        public Guid WardId { get; set; }
        public Ward Ward { get; set; } = null!;
        public Guid RoomTypeId { get; set; }
        public int Floor { get; set; }
        public RoomType RoomType { get; set; } = null!;
        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
    }
}