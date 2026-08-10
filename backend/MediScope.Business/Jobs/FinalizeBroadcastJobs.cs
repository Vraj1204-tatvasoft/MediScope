using Hangfire;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using MediScope.Business.Hubs;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.Business.Jobs
{
    /// <summary>
    /// Finalizer job. Runs once after the last ProcessBatchJob completes.
    ///
    /// Since the database function fn_decrement_remaining_batches safely handles
    /// concurrent atomic increments for sent and failed counts, this job only
    /// needs to read those properties directly from the broadcast record.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public class FinalizeBroadcastJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly ILogger<FinalizeBroadcastJob> _logger;
        private readonly IHubContext<RealtimeHub> _hubContext;

        public FinalizeBroadcastJob(
            IBroadcastRepository repository,
            ILogger<FinalizeBroadcastJob> logger,
            IHubContext<RealtimeHub> hubContext)
        {
            _repository = repository;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task ExecuteAsync(Guid broadcastId, CancellationToken ct)
        {
            _logger.LogInformation("FinalizeBroadcastJob: finalizing broadcast {Id}", broadcastId);

            // Fetch the broadcast which now holds the atomically updated SentCount and FailedCount
            var broadcast = await _repository.GetBroadcastByIdAsync(broadcastId);
            if (broadcast is null)
            {
                _logger.LogWarning("FinalizeBroadcastJob: broadcast {Id} not found.", broadcastId);
                return;
            }

            int sentCount = broadcast.SentCount;
            int failedCount = broadcast.FailedCount;

            var finalStatus = sentCount == 0 && failedCount > 0
                ? BroadcastStatus.Failed
                : BroadcastStatus.Completed;

            await _repository.CompleteBroadcastAsync(broadcastId, sentCount, failedCount, finalStatus);

            await _hubContext.Clients.All.SendAsync("BroadcastStatusUpdated", new
            {
                BroadcastId = broadcastId,
                Status = finalStatus.ToString(),
                TotalSent = sentCount,
                TotalFailed = failedCount
            }, ct);

            _logger.LogInformation("FinalizeBroadcastJob: broadcast {Id} → {Status} (Sent={Sent} Failed={Failed})",
                broadcastId, finalStatus, sentCount, failedCount);
        }
    }
}