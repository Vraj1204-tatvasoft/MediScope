using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Enums;
namespace MediScope.Business.Services.Interfaces
{
    public interface IBroadcastService
    {
        Task<Guid> CreateBroadcastAsync(CreateBroadcastRequestDto request, Guid userId);
        Task UpdateBroadcastAsync(Guid id, UpdateBroadcastRequestDto request);
        Task SoftDeleteBroadcastAsync(Guid id);
        Task<BroadcastResponseDto?> GetBroadcastByIdAsync(Guid id);
        Task<BroadcastPagedResponseDto> GetBroadcastsPagedAsync(GetBroadcastsRequestDto request);
        Task<int> SendBroadcastAsync(Guid broadcastId);
        Task<int> RetryBroadcastAsync(Guid broadcastId);
        Task<AudienceCountResponseDto> GetAudienceCountAsync(BroadcastAudience audience);
    }
}