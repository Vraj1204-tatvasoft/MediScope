
namespace MediScope.Common.Models.Entities
{
    public class Ward : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}