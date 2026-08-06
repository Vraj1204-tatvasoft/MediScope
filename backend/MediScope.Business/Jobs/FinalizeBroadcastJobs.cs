using Hangfire;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.Business.Jobs
{
    /// <summary>
    /// Fix 3: Finalizer job chained via ContinueJobWith onto the last
    /// ProcessBatchJob in BroadcastDispatchJob.
    ///
    /// Why this is needed:
    ///   ProcessBatchJob instances run concurrently. There is no safe way
    ///   to accumulate sent/failed counts across them without shared state
    ///   or locking. Instead this job runs after all of them finish and
    ///   reads the true counts directly from broadcast_recipients rows —
    ///   a single aggregation query with no race conditions.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public class FinalizeBroadcastJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly ILogger<FinalizeBroadcastJob> _logger;

        public FinalizeBroadcastJob(
            IBroadcastRepository repository,
            ILogger<FinalizeBroadcastJob> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid broadcastId, CancellationToken ct)
        {
            _logger.LogInformation("FinalizeBroadcastJob: reading final counts for broadcast {Id}", broadcastId);

            var (sentCount, failedCount) = await _repository.GetFinalCountsAsync(broadcastId);

            var finalStatus = sentCount == 0 && failedCount > 0
                ? BroadcastStatus.Failed
                : BroadcastStatus.Completed;

            await _repository.CompleteBroadcastAsync(
                broadcastId, sentCount, failedCount, finalStatus);

            _logger.LogInformation("FinalizeBroadcastJob: broadcast {Id} → {Status} (Sent={Sent} Failed={Failed})", broadcastId, finalStatus, sentCount, failedCount);
        }
    }
}