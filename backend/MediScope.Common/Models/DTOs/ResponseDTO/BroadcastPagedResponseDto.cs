using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Response
{
    public class BroadcastPagedResponseDto
    {
        public List<BroadcastListItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}