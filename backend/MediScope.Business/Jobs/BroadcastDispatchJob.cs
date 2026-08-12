using Hangfire;
using MediScope.Business.Hubs;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MediScope.Business.Jobs
{
    /// <summary>
    /// Orchestrator job. Runs once per broadcast Send.
    ///
    /// Enqueues all ProcessBatchJob instances in parallel.
    /// No ContinueJobWith chaining. No FinalizeBroadcastJob.
    /// Finalization is handled inside ProcessBatchJob using
    /// fn_update_broadcast_counts which atomically tracks progress
    /// against total_recipients — no new columns required.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public class BroadcastDispatchJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly IBackgroundJobClient _jobClient;
        private readonly ILogger<BroadcastDispatchJob> _logger;
        private readonly IHubContext<RealtimeHub> _hubContext;

        public BroadcastDispatchJob(
            IBroadcastRepository repository,
            IBackgroundJobClient jobClient,
            ILogger<BroadcastDispatchJob> logger,
            IHubContext<RealtimeHub> hubContext)
        {
            _repository = repository;
            _jobClient = jobClient;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task ExecuteAsync(Guid broadcastId, CancellationToken ct)
        {
            var broadcast = await _repository.GetBroadcastByIdAsync(broadcastId);
            if (broadcast is null)
            {
                _logger.LogError("BroadcastDispatchJob: broadcast {Id} not found.", broadcastId);
                return;
            }

            if (broadcast.Status == BroadcastStatus.Processing)
            {
                _logger.LogWarning(
                    "BroadcastDispatchJob: broadcast {Id} is already Processing. Aborting.",
                    broadcastId);
                return;
            }

            _logger.LogInformation("BroadcastDispatchJob starting — Id={Id} Channel={Channel} Audience={Audience} BatchSize={BatchSize}",
                broadcastId, broadcast.Channel, broadcast.Audience, broadcast.BatchSize);

            await _repository.MarkProcessingAsync(broadcastId);

            await _hubContext.Clients.All.SendAsync("BroadcastUpdated", new
            {
                id = broadcastId,
                status = "Processing",
                statusDisplay = "Processing"
            }, ct);

            try
            {
                int batchNumber = 1;
                int batchSize = broadcast.BatchSize;
                int offset = 0;
                var batchNumbers = new List<int>();

                // Phase 1: insert all recipient batches
                while (true)
                {
                    var contacts = await _repository.GetAudienceContactsBatchAsync(broadcast.Audience, offset, batchSize);

                    if (contacts.Count == 0) break;

                    await _repository.BulkInsertRecipientsAsync(broadcastId, contacts, batchNumber);

                    batchNumbers.Add(batchNumber);

                    _logger.LogInformation("BroadcastDispatchJob: inserted batch {Batch} — {Count} recipients", batchNumber, contacts.Count);

                    if (contacts.Count < batchSize) break;

                    offset += batchSize;
                    batchNumber++;
                }

                if (batchNumbers.Count == 0)
                {
                    await _repository.CompleteBroadcastAsync(
                        broadcastId, 0, 0, BroadcastStatus.Failed, "No contacts returned during batch fetch.");

                    await _hubContext.Clients.All.SendAsync("BroadcastStatusUpdated", new
                    {
                        BroadcastId = broadcastId,
                        Status = "Failed",
                        StatusDisplay = "Failed",
                        TotalSent = 0,
                        TotalFailed = 0
                    }, ct);

                    return;
                }

                // Phase 2: enqueue all batch jobs in parallel.
                // ProcessBatchJob handles finalization via fn_update_broadcast_counts.
                foreach (var bn in batchNumbers)
                {
                    _jobClient.Enqueue<ProcessBatchJob>(
                        job => job.ExecuteAsync(broadcastId, bn, CancellationToken.None));
                }

                _logger.LogInformation(
                    "BroadcastDispatchJob: enqueued {Total} parallel batch job(s) for broadcast {Id}",
                    batchNumbers.Count, broadcastId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "BroadcastDispatchJob faulted — Id={Id}", broadcastId);

                await _repository.CompleteBroadcastAsync(
                    broadcastId, 0, 0, BroadcastStatus.Failed, ex.Message);

                await _hubContext.Clients.All.SendAsync("BroadcastStatusUpdated", new
                {
                    BroadcastId = broadcastId,
                    Status = "Failed",
                    StatusDisplay = "Failed",
                    TotalSent = 0,
                    TotalFailed = 0
                }, ct);
            }
        }
    }
}