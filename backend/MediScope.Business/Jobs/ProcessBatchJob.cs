using Hangfire;
using MediScope.Business.Hubs;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.Business.Jobs
{
    /// <summary>
    /// Child Hangfire job. One instance per batch number per broadcast.
    /// All instances run in parallel across Hangfire workers.
    ///
    /// Finalization is integrated here — no separate FinalizeBroadcastJob.
    ///
    /// After processing all recipients in its batch, each job calls
    /// fn_update_broadcast_counts which atomically increments sent_count
    /// and failed_count and returns TRUE when sent+failed = total_recipients.
    /// PostgreSQL row locking guarantees exactly one worker receives TRUE.
    /// That worker reads the final counts and calls sp_complete_broadcast.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public class ProcessBatchJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly IEmailService _emailSender;
        private readonly ISmsService _smsSender;
        private readonly IPushService _pushSender;
        private readonly IHubContext<RealtimeHub> _hubContext;
        private readonly ILogger<ProcessBatchJob> _logger;

        public ProcessBatchJob(
            IBroadcastRepository repository,
            IEmailService emailSender,
            ISmsService smsSender,
            IPushService pushSender,
            IHubContext<RealtimeHub> hubContext,
            ILogger<ProcessBatchJob> logger)
        {
            _repository = repository;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _pushSender = pushSender;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid broadcastId, int batchNumber, CancellationToken ct)
        {
            // Wrap entire method so job always reaches Succeeded state.
            // If this threw, Hangfire marks it Failed and the atomic counter
            // never decrements for this batch, leaving broadcast stuck in Processing.
            try
            {
                var broadcast = await _repository.GetBroadcastByIdAsync(broadcastId);
                if (broadcast is null)
                {
                    _logger.LogError(
                        "ProcessBatchJob: broadcast {Id} not found. Batch={Batch}",
                        broadcastId, batchNumber);
                    await FinalizeIfLastBatchAsync(broadcastId, 0, 0, ct);
                    return;
                }

                var recipients = await _repository.GetRecipientsByBatchAsync(broadcastId, batchNumber);

                if (recipients.Count == 0)
                {
                    _logger.LogInformation(
                        "ProcessBatchJob: no Pending recipients in batch {Batch} for broadcast {Id}.",
                        batchNumber, broadcastId);

                    await FinalizeIfLastBatchAsync(broadcastId, 0, 0, ct);
                    return;
                }

                _logger.LogInformation(
                    "ProcessBatchJob: sending batch {Batch} — {Count} recipients for broadcast {Id}",
                    batchNumber, recipients.Count, broadcastId);

                int sent = 0;
                int failed = 0;

                foreach (var recipient in recipients)
                {
                    bool success = await DispatchAsync(broadcast, recipient, ct);
                    if (success) sent++;
                    else failed++;
                }

                _logger.LogInformation(
                    "ProcessBatchJob: batch {Batch} done — Sent={Sent} Failed={Failed} for broadcast {Id}",
                    batchNumber, sent, failed, broadcastId);

                // Atomically update counts and check if this is the last batch.
                await FinalizeIfLastBatchAsync(broadcastId, sent, failed, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ProcessBatchJob: unhandled exception in batch {Batch} for broadcast {Id}",
                    batchNumber, broadcastId);

                await FinalizeIfLastBatchAsync(broadcastId, 0, 0, ct);
            }
        }

        /// <summary>
        /// Calls fn_update_broadcast_counts atomically.
        /// Only the worker that receives TRUE runs CompleteBroadcastAsync.
        /// All other workers receive FALSE and exit without doing anything.
        /// </summary>
        private async Task FinalizeIfLastBatchAsync(Guid broadcastId, int sent, int failed, CancellationToken ct)
        {
            bool isLastBatch = await _repository.UpdateBroadcastCountsAsync(
                broadcastId, sent, failed);

            if (!isLastBatch) return;

            // This worker is the last one — read ground-truth counts from
            // broadcast_recipients and write the final status.
            _logger.LogInformation(
                "ProcessBatchJob: last batch completed for broadcast {Id}, finalizing.",
                broadcastId);

            var (sentCount, failedCount) = await _repository.GetFinalCountsAsync(broadcastId);

            // Completed = at least one message sent (partial success is still Completed).
            // Failed    = nothing got through at all.
            var finalStatus = sentCount == 0 && failedCount > 0
                ? BroadcastStatus.Failed
                : BroadcastStatus.Completed;

            await _repository.CompleteBroadcastAsync(broadcastId, sentCount, failedCount, finalStatus);

            await _hubContext.Clients.All.SendAsync("BroadcastStatusUpdated", new
            {
                BroadcastId = broadcastId,
                Status = finalStatus.ToString(),
                StatusDisplay = finalStatus.ToString(),
                TotalSent = sentCount,
                TotalFailed = failedCount
            }, ct);

            _logger.LogInformation(
                "ProcessBatchJob: broadcast {Id} finalized → {Status} (Sent={Sent} Failed={Failed})",
                broadcastId, finalStatus, sentCount, failedCount);
        }

        // ── Dispatch ──────────────────────────────────────────────────────────

        private Task<bool> DispatchAsync(Broadcast broadcast, BroadcastRecipientRow recipient, CancellationToken ct)
        {
            return broadcast.Channel switch
            {
                BroadcastChannel.Email => SendEmailAsync(broadcast, recipient),
                BroadcastChannel.Sms => SendSmsAsync(recipient),
                BroadcastChannel.PushNotification => SendPushAsync(broadcast, recipient, ct),
                _ => Task.FromResult(false)
            };
        }

        private async Task<bool> SendEmailAsync(Broadcast broadcast, BroadcastRecipientRow recipient)
        {
            try
            {
                await _emailSender.SendAsync(
                    recipient.Email,
                    broadcast.Subject ?? broadcast.Name,
                    broadcast.Message);

                await _repository.UpdateRecipientStatusAsync(
                    recipient.Id, RecipientStatus.Sent);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "ProcessBatchJob: email failed → {Email}: {Error}",
                    recipient.Email, ex.Message);

                await _repository.UpdateRecipientStatusAsync(
                    recipient.Id, RecipientStatus.Failed, ex.Message);

                return false;
            }
        }

        private async Task<bool> SendSmsAsync(BroadcastRecipientRow recipient)
        {
            _logger.LogWarning(
                "ProcessBatchJob: SMS not yet supported. Recipient={Id}", recipient.Id);

            await _repository.UpdateRecipientStatusAsync(
                recipient.Id, RecipientStatus.Failed,
                "SMS not yet supported.");

            return false;
        }

        private async Task<bool> SendPushAsync(Broadcast broadcast, BroadcastRecipientRow recipient, CancellationToken ct)
        {
            var (success, error) = await _pushSender.SendAsync(
                recipient.UserId,
                broadcast.Subject ?? broadcast.Name,
                broadcast.Message,
                ct);

            if (success)
            {
                await _repository.UpdateRecipientStatusAsync(
                    recipient.Id, RecipientStatus.Sent);
            }
            else
            {
                _logger.LogWarning(
                    "ProcessBatchJob: push failed → UserId={UserId}: {Error}",
                    recipient.UserId, error);

                await _repository.UpdateRecipientStatusAsync(
                    recipient.Id, RecipientStatus.Failed, error);
            }

            return success;
        }
    }
}