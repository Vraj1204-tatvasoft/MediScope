using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Request
{
    public class UpdateBedRequestDto
    {
        public string BedNumber { get; set; } = string.Empty;
        public BedStatus Status { get; set; }
    }
}