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
    /// Orchestrator job. Runs once per broadcast Send.
    ///
    /// Prepares all recipient batches, sets the total remaining batches in the database,
    /// and then enqueues all ProcessBatchJobs to run in parallel.
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
                _logger.LogWarning("BroadcastDispatchJob: broadcast {Id} is already Processing. Aborting.", broadcastId);
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
                int batchNumber = 0;
                int batchSize = broadcast.BatchSize;
                int offset = 0;

                // 1. Fetch and insert all contacts to determine total batches BEFORE enqueueing.
                // This prevents race conditions where a batch finishes before the total is set.
                while (true)
                {
                    var contacts = await _repository.GetAudienceContactsBatchAsync(broadcast.Audience, offset, batchSize);
                    if (contacts.Count == 0) break;

                    batchNumber++;
                    await _repository.BulkInsertRecipientsAsync(broadcastId, contacts, batchNumber);

                    if (contacts.Count < batchSize) break;
                    offset += batchSize;
                }

                int totalBatches = batchNumber;

                if (totalBatches > 0)
                {
                    // 2. Lock in the total batches using the stored procedure
                    await _repository.SetRemainingBatchesAsync(broadcastId, totalBatches);

                    // 3. Enqueue all batch jobs to be processed in parallel by Hangfire workers
                    for (int i = 1; i <= totalBatches; i++)
                    {
                        // Capture the loop variable locally to avoid closure issues in the lambda
                        int currentBatch = i;
                        _jobClient.Enqueue<ProcessBatchJob>(job => job.ExecuteAsync(broadcastId, currentBatch, CancellationToken.None));
                    }

                    _logger.LogInformation("BroadcastDispatchJob: enqueued {TotalBatches} parallel batch job(s) for broadcast {Id}",
                        totalBatches, broadcastId);
                }
                else
                {
                    await _repository.CompleteBroadcastAsync(broadcastId, 0, 0, BroadcastStatus.Failed, "No contacts returned during batch fetch.");
                    await _hubContext.Clients.All.SendAsync("BroadcastUpdated", new
                    {
                        id = broadcastId,
                        status = "Failed",
                        statusDisplay = "Failed"
                    }, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BroadcastDispatchJob faulted during batch setup — Id={Id}", broadcastId);

                await _repository.CompleteBroadcastAsync(broadcastId, 0, 0, BroadcastStatus.Failed, ex.Message);
                await _hubContext.Clients.All.SendAsync("BroadcastUpdated", new
                {
                    id = broadcastId,
                    status = "Failed",
                    statusDisplay = "Failed"
                }, ct);
            }
        }
    }
}