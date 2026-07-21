using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Request
{
    public class TransferBedRequestDto
    {
        public Guid NewWardId { get; set; }
        public Guid NewRoomId { get; set; }
        public Guid NewBedId { get; set; }
        public string TransferReason { get; set; } = string.Empty;
    }
}