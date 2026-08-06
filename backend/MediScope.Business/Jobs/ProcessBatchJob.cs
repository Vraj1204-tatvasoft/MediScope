using Hangfire;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Logging;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
namespace MediScope.Business.Jobs
{
    /// <summary>
    /// Child Hangfire job. One instance per batch number per broadcast.
    ///
    /// Flow:
    ///   1. Load Pending recipients for this broadcastId + batchNumber
    ///      from broadcast_recipients via fn_get_recipients_by_batch.
    ///   2. Send each recipient concurrently via Task.WhenAll.
    ///   3. Update each recipient's status (Sent / Failed) + error message.
    ///   4. Update broadcast.sent_count / failed_count via sp_complete_broadcast
    ///      is NOT called here — FinalizeBroadcastJob does that after all
    ///      child jobs are done.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public class ProcessBatchJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly IEmailService _emailSender;
        private readonly ISmsService _smsSender;
        private readonly IPushService _pushSender;
        private readonly ILogger<ProcessBatchJob> _logger;

        public ProcessBatchJob(
            IBroadcastRepository repository,
            IEmailService emailSender,
            ISmsService smsSender,
            IPushService pushSender,
            ILogger<ProcessBatchJob> logger)
        {
            _repository = repository;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _pushSender = pushSender;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid broadcastId, int batchNumber, CancellationToken ct)
        {
            var broadcast = await _repository.GetBroadcastByIdAsync(broadcastId);
            if (broadcast is null)
            {
                _logger.LogError(
                    "ProcessBatchJob: broadcast {Id} not found. Batch={Batch}",
                    broadcastId, batchNumber);
                return;
            }

            // Load only Pending recipients for this batch
            var recipients = await _repository.GetRecipientsByBatchAsync(broadcastId, batchNumber);

            if (recipients.Count == 0)
            {
                _logger.LogInformation(
                    "ProcessBatchJob: no Pending recipients in batch {Batch} for broadcast {Id}.",
                    batchNumber, broadcastId);
                return;
            }

            _logger.LogInformation(
                "ProcessBatchJob: sending batch {Batch} — {Count} recipients for broadcast {Id}",
                batchNumber, recipients.Count, broadcastId);

            int sent = 0;
            int failed = 0;

            // Sequential foreach — avoids concurrent DbContext access.
            foreach (var recipient in recipients)
            {
                bool success = await DispatchAsync(broadcast, recipient, ct);

                if (success) sent++;
                else failed++;
            }

            _logger.LogInformation(
                "ProcessBatchJob: batch {Batch} done for broadcast {Id}",
                batchNumber, broadcastId);
        }

        // ── Dispatch to the correct channel ───────────────────────────────────

        private Task<bool> DispatchAsync(Broadcast broadcast, BroadcastRecipientRow recipient, CancellationToken ct)
        {
            return broadcast.Channel switch
            {
                BroadcastChannel.Email => SendEmailAsync(broadcast, recipient),
                BroadcastChannel.Sms => SendSmsAsync(broadcast, recipient),
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

        private async Task<bool> SendSmsAsync(Broadcast broadcast, BroadcastRecipientRow recipient)
        {
            // fn_get_audience_contacts_batch returns email only.
            // Extend that function to include phone when SMS is implemented.
            _logger.LogWarning(
                "ProcessBatchJob: SMS not yet supported. Marking recipient {Id} as Failed.",
                recipient.Id);

            await _repository.UpdateRecipientStatusAsync(
                recipient.Id, RecipientStatus.Failed,
                "SMS not yet supported. Extend fn_get_audience_contacts_batch to include phone.");
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
                    "ProcessBatchJob: push failed → UserId {UserId}: {Error}",
                    recipient.UserId, error);

                await _repository.UpdateRecipientStatusAsync(
                    recipient.Id, RecipientStatus.Failed, error);
            }
            return success;
        }
    }
}