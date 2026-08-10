using Hangfire;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using MediScope.Business.Hubs;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.Business.Jobs
{
    [AutomaticRetry(Attempts = 0)]
    public class ProcessBatchJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly IEmailService _emailSender;
        private readonly ISmsService _smsSender;
        private readonly IPushService _pushSender;
        private readonly IBackgroundJobClient _jobClient;
        private readonly ILogger<ProcessBatchJob> _logger;
        private readonly IHubContext<RealtimeHub> _hubContext;

        public ProcessBatchJob(
            IBroadcastRepository repository,
            IEmailService emailSender,
            ISmsService smsSender,
            IPushService pushSender,
            IBackgroundJobClient jobClient,
            ILogger<ProcessBatchJob> logger,
            IHubContext<RealtimeHub> hubContext)
        {
            _repository = repository;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _pushSender = pushSender;
            _jobClient = jobClient;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task ExecuteAsync(Guid broadcastId, int batchNumber, CancellationToken ct)
        {
            int sent = 0;
            int failed = 0;

            try
            {
                var broadcast = await _repository.GetBroadcastByIdAsync(broadcastId);
                if (broadcast is null)
                {
                    _logger.LogError("ProcessBatchJob: broadcast {Id} not found. Batch={Batch}", broadcastId, batchNumber);
                    return;
                }

                var recipients = await _repository.GetRecipientsByBatchAsync(broadcastId, batchNumber);

                if (recipients.Count == 0)
                {
                    _logger.LogInformation("ProcessBatchJob: no Pending recipients in batch {Batch} for broadcast {Id}.", batchNumber, broadcastId);
                }
                else
                {
                    _logger.LogInformation("ProcessBatchJob: sending batch {Batch} — {Count} recipients for broadcast {Id}", batchNumber, recipients.Count, broadcastId);

                    foreach (var recipient in recipients)
                    {
                        bool success = await DispatchAsync(broadcast, recipient, ct);
                        if (success) sent++;
                        else failed++;
                    }

                    if (sent > 0 || failed > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("BroadcastProgressUpdated", new
                        {
                            BroadcastId = broadcastId,
                            BatchNumber = batchNumber,
                            Sent = sent,
                            Failed = failed
                        }, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessBatchJob: unhandled exception in batch {Batch} for broadcast {Id}", batchNumber, broadcastId);
            }
            finally
            {
                // Decrement remaining_batches and increment sent/failed counts atomically in PostgreSQL.
                // Runs inside 'finally' so batch execution always updates the counter even on failure or empty recipients.
                int remainingBatches = await _repository.DecrementRemainingBatchesAsync(broadcastId, sent, failed);

                _logger.LogInformation("ProcessBatchJob: batch {Batch} done — Sent={Sent} Failed={Failed} RemainingBatches={Remaining} for broadcast {Id}",
                    batchNumber, sent, failed, remainingBatches, broadcastId);

                // The worker that finishes the absolute LAST batch triggers FinalizeBroadcastJob
                if (remainingBatches == 0)
                {
                    _logger.LogInformation("ProcessBatchJob: Last batch ({Batch}) finished for broadcast {Id}. Triggering FinalizeBroadcastJob.",
                        batchNumber, broadcastId);

                    _jobClient.Enqueue<FinalizeBroadcastJob>(job => job.ExecuteAsync(broadcastId, CancellationToken.None));
                }
            }
        }

        // Dispatch 

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
            _logger.LogWarning("ProcessBatchJob: SMS not yet supported. Recipient={Id}", recipient.Id);

            await _repository.UpdateRecipientStatusAsync(recipient.Id, RecipientStatus.Failed, "SMS not yet supported.");

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
                _logger.LogWarning("ProcessBatchJob: push failed → UserId={UserId}: {Error}", recipient.UserId, error);

                await _repository.UpdateRecipientStatusAsync(recipient.Id, RecipientStatus.Failed, error);
            }

            return success;
        }
    }
}