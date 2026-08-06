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
    [AutomaticRetry(Attempts = 0)]
    public class RetryBatchJob
    {
        private readonly IBroadcastRepository _repository;
        private readonly IEmailService _emailSender;
        private readonly ISmsService _smsSender;
        private readonly IPushService _pushSender;
        private readonly ILogger<RetryBatchJob> _logger;

        public RetryBatchJob(
            IBroadcastRepository repository,
            IEmailService emailSender,
            ISmsService smsSender,
            IPushService pushSender,
            ILogger<RetryBatchJob> logger)
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
                    "RetryBatchJob: broadcast {Id} not found. Batch={Batch}",
                    broadcastId, batchNumber);
                return;
            }

            // Load all failed recipients for this broadcast then filter by batchNumber
            var allFailed = await _repository.GetFailedRecipientsAsync(broadcastId);
            var recipients = allFailed.Where(r => r.BatchNumber == batchNumber).ToList();

            if (recipients.Count == 0)
            {
                _logger.LogInformation(
                    "RetryBatchJob: no Failed recipients in batch {Batch} for broadcast {Id}.",
                    batchNumber, broadcastId);
                return;
            }

            _logger.LogInformation(
                "RetryBatchJob: retrying batch {Batch} — {Count} failed recipients for broadcast {Id}",
                batchNumber, recipients.Count, broadcastId);

            // Increment retry_count and reset each recipient to Pending before
            // attempting sends — state stays consistent even if job is interrupted
            foreach (var recipient in recipients)
            {
                await _repository.IncrementRetryCountAsync(recipient.Id);
            }

            int sent = 0;
            int failed = 0;

            foreach (var recipient in recipients)
            {
                bool success = await DispatchAsync(broadcast, recipient, ct);

                if (success) sent++;
                else failed++;
            }

            _logger.LogInformation(
                "RetryBatchJob: batch {Batch} retry complete for broadcast {Id}",
                batchNumber, broadcastId);
        }

        // ── Dispatch ──────────────────────────────────────────────────────────

        private Task<bool> DispatchAsync(
            Broadcast broadcast, BroadcastRecipientRow recipient, CancellationToken ct)
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
                    "RetryBatchJob: email failed → {Email}: {Error}",
                    recipient.Email, ex.Message);

                await _repository.UpdateRecipientStatusAsync(
                    recipient.Id, RecipientStatus.Failed, ex.Message);
                return false;
            }
        }

        private async Task<bool> SendSmsAsync(BroadcastRecipientRow recipient)
        {
            _logger.LogWarning(
                "RetryBatchJob: SMS not yet supported. Marking recipient {Id} as Failed.",
                recipient.Id);

            await _repository.UpdateRecipientStatusAsync(
                recipient.Id, RecipientStatus.Failed, "SMS not yet supported.");

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
                    "RetryBatchJob: push failed → UserId {UserId}: {Error}",
                    recipient.UserId, error);

                await _repository.UpdateRecipientStatusAsync(
                    recipient.Id, RecipientStatus.Failed, error);
            }
            return success;
        }
    }
}