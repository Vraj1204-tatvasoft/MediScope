using System.ComponentModel.DataAnnotations;
using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Request
{
    public class GetBroadcastsRequestDto
    {
        public string? Search { get; set; }
        public BroadcastChannel? Channel { get; set; }
        public BroadcastStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}