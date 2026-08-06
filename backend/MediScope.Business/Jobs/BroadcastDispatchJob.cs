using Hangfire;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.Business.Jobs
{
    /// <summary>
    /// Orchestrator job. Runs once per broadcast Send.
    ///
    /// Fix 3: After enqueuing all ProcessBatchJob child jobs, chains
    /// FinalizeBroadcastJob onto the last child job ID using ContinueJobWith.
    /// FinalizeBroadcastJob only executes after that last batch finishes,
    /// by which point all earlier batches have also completed.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public class BroadcastDispatchJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly IBackgroundJobClient _jobClient;
        private readonly ILogger<BroadcastDispatchJob> _logger;

        public BroadcastDispatchJob(
            IBroadcastRepository repository,
            IBackgroundJobClient jobClient,
            ILogger<BroadcastDispatchJob> logger)
        {
            _repository = repository;
            _jobClient = jobClient;
            _logger = logger;
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

            _logger.LogInformation(
                "BroadcastDispatchJob starting — Id={Id} Channel={Channel} Audience={Audience} BatchSize={BatchSize}",
                broadcastId, broadcast.Channel, broadcast.Audience, broadcast.BatchSize);

            await _repository.MarkProcessingAsync(broadcastId);

            try
            {
                int batchNumber = 1;
                int batchSize = broadcast.BatchSize;
                int offset = 0;
                string? lastJobId = null;

                while (true)
                {
                    var contacts = await _repository.GetAudienceContactsBatchAsync(
                        broadcast.Audience, offset, batchSize);

                    if (contacts.Count == 0)
                        break;

                    _logger.LogInformation(
                        "BroadcastDispatchJob: inserting batch {Batch} — {Count} recipients",
                        batchNumber, contacts.Count);

                    await _repository.BulkInsertRecipientsAsync(broadcastId, contacts, batchNumber);
                    lastJobId = _jobClient.Enqueue<ProcessBatchJob>(
                        job => job.ExecuteAsync(broadcastId, batchNumber, CancellationToken.None));

                    if (contacts.Count < batchSize)
                        break;

                    offset += batchSize;
                    batchNumber++;
                }

                // Fix 3: chain FinalizeBroadcastJob onto the last ProcessBatchJob.
                // Hangfire will only start it after lastJobId reaches a terminal state
                // (Succeeded). All earlier batch jobs finish before the last one
                // because they were enqueued in order on the same queue.
                if (lastJobId is not null)
                {
                    _jobClient.ContinueJobWith<FinalizeBroadcastJob>(
                        lastJobId,
                        job => job.ExecuteAsync(broadcastId, CancellationToken.None));

                    _logger.LogInformation(
                        "BroadcastDispatchJob: enqueued {TotalBatches} batch job(s) + finalizer for broadcast {Id}",
                        batchNumber, broadcastId);
                }
                else
                {
                    await _repository.CompleteBroadcastAsync(
                        broadcastId, 0, 0, BroadcastStatus.Failed,
                        "No contacts returned during batch fetch.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "BroadcastDispatchJob faulted during batch setup — Id={Id}", broadcastId);

                await _repository.CompleteBroadcastAsync(
                    broadcastId, 0, 0, BroadcastStatus.Failed, ex.Message);
            }
        }
    }
}