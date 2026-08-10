using MediScope.Common.Models.Entities;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Enums;
namespace MediScope.Data.Repositories
{
    public interface IBroadcastRepository
    {
        Task CreateBroadcastAsync(Broadcast broadcast);
        Task UpdateBroadcastAsync(Broadcast broadcast);
        Task<Broadcast?> GetBroadcastByIdAsync(Guid id);
        Task<BroadcastPagedResponseDto> GetBroadcastsPagedAsync(GetBroadcastsRequestDto request);
        Task SoftDeleteBroadcastAsync(Guid id);
        Task MarkPendingAsync(Guid broadcastId, string hangfireJobId, int totalRecipients);
        Task MarkProcessingAsync(Guid broadcastId);
        Task CompleteBroadcastAsync(Guid broadcastId, int sentCount, int failedCount, BroadcastStatus status, string? failureReason = null);
        Task<List<DbAudienceMember>> GetAudienceContactsBatchAsync(BroadcastAudience audience, int offset, int limit);
        Task<int> GetAudienceCountAsync(BroadcastAudience audience);
        Task BulkInsertRecipientsAsync(Guid broadcastId, List<DbAudienceMember> contacts, int batchNumber);
        Task UpdateRecipientStatusAsync(Guid recipientId, RecipientStatus status, string? errorMessage = null);
        Task<List<BroadcastRecipientRow>> GetRecipientsByBatchAsync(Guid broadcastId, int batchNumber);
        Task<List<BroadcastRecipientRow>> GetFailedRecipientsAsync(Guid broadcastId);
        Task IncrementRetryCountAsync(Guid recipientId);
        Task<(int SentCount, int FailedCount)> GetFinalCountsAsync(Guid broadcastId);
        Task IncrementBroadcastCountsAsync(Guid broadcastId, int sentDelta, int failedDelta);
        Task SetRemainingBatchesAsync(Guid broadcastId, int totalBatches);
        Task<int> DecrementRemainingBatchesAsync(Guid broadcastId, int sentDelta, int failedDelta);
    }
}