using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.Entities
{
    public class Bed : BaseEntity
    {
        public string BedNumber { get; set; } = string.Empty;
        public BedStatus Status { get; set; } = BedStatus.Available;
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;
    }
}