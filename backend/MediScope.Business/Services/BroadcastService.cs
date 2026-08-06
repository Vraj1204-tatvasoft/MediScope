using Hangfire;
using MediScope.Business.Jobs;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Data.Repositories;
using MediScope.Business.Services.Interfaces;

namespace MediScope.Business.Services
{
    public class BroadcastService : IBroadcastService
    {
        private readonly IBroadcastRepository _broadcastRepository;
        private readonly IBackgroundJobClient _jobClient;

        public BroadcastService(IBroadcastRepository broadcastRepository, IBackgroundJobClient jobClient)
        {
            _broadcastRepository = broadcastRepository;
            _jobClient = jobClient;
        }

        public async Task<Guid> CreateBroadcastAsync(CreateBroadcastRequestDto request, Guid userId)
        {
            var broadcast = new Broadcast
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Channel = request.Channel,
                Subject = request.Subject,
                Message = request.Message,
                Audience = request.Audience,
                BatchSize = request.BatchSize,
                CreatedBy = userId,
                Status = request.SendNow ? BroadcastStatus.Pending : BroadcastStatus.Draft
            };

            await _broadcastRepository.CreateBroadcastAsync(broadcast);

            return broadcast.Id;
        }

        public async Task UpdateBroadcastAsync(Guid id, UpdateBroadcastRequestDto request)
        {
            var existing = await _broadcastRepository.GetBroadcastByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Broadcast with ID {id} was not found.");
            }

            var broadcast = new Broadcast
            {
                Id = id,
                Name = request.Name,
                Channel = request.Channel,
                Subject = request.Subject,
                Message = request.Message,
                Audience = request.Audience,
                Status = existing.Status
            };

            await _broadcastRepository.UpdateBroadcastAsync(broadcast);
        }

        public async Task SoftDeleteBroadcastAsync(Guid id)
        {
            var existing = await _broadcastRepository.GetBroadcastByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Broadcast with ID {id} was not found.");
            }

            await _broadcastRepository.SoftDeleteBroadcastAsync(id);
        }

        public async Task<BroadcastResponseDto?> GetBroadcastByIdAsync(Guid id)
        {
            var entity = await _broadcastRepository.GetBroadcastByIdAsync(id);

            if (entity == null) return null;

            return new BroadcastResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Channel = entity.Channel,
                Subject = entity.Subject,
                Message = entity.Message,
                Audience = entity.Audience,
                Status = entity.Status,
                BatchSize = entity.BatchSize,
                CreatedBy = entity.CreatedBy ?? Guid.Empty,
                CreatedAt = entity.CreatedAt,
                TotalRecipients = entity.TotalRecipients,
                SentCount = entity.SentCount,
                FailedCount = entity.FailedCount,
                ScheduledAt = entity.ScheduledAt,
                CompletedAt = entity.CompletedAt
            };
        }

        public async Task<BroadcastPagedResponseDto> GetBroadcastsPagedAsync(GetBroadcastsRequestDto request)
        {
            return await _broadcastRepository.GetBroadcastsPagedAsync(request);
        }

        public async Task<int> SendBroadcastAsync(Guid broadcastId)
        {
            var existing = await _broadcastRepository.GetBroadcastByIdAsync(broadcastId)
                ?? throw new KeyNotFoundException($"Broadcast with ID {broadcastId} was not found.");

            if (existing.Status is BroadcastStatus.Pending or BroadcastStatus.Processing)
                throw new InvalidOperationException("Broadcast is already queued or processing.");

            // if (existing.Status == BroadcastStatus.Completed)
            //     throw new InvalidOperationException("Broadcast has already been completed.");

            int totalRecipients = await _broadcastRepository.GetAudienceCountAsync(existing.Audience);

            if (totalRecipients == 0)
                throw new InvalidOperationException($"No eligible recipients found for audience '{existing.Audience}'.");

            string jobId = _jobClient.Enqueue<BroadcastDispatchJob>(job => job.ExecuteAsync(broadcastId, CancellationToken.None));

            await _broadcastRepository.MarkPendingAsync(broadcastId, jobId, totalRecipients);

            return totalRecipients;
        }
        public async Task<int> RetryBroadcastAsync(Guid broadcastId)
        {
            var existing = await _broadcastRepository.GetBroadcastByIdAsync(broadcastId)
                ?? throw new KeyNotFoundException($"Broadcast with ID {broadcastId} was not found.");

            if (existing.Status is BroadcastStatus.Pending or BroadcastStatus.Processing)
                throw new InvalidOperationException("Cannot retry a broadcast that is currently processing.");

            var failedRecipients = await _broadcastRepository.GetFailedRecipientsAsync(broadcastId);

            if (failedRecipients.Count == 0)
                throw new InvalidOperationException("No failed recipients found for this broadcast.");

            var batchGroups = failedRecipients
                .GroupBy(r => r.BatchNumber)
                .ToList();

            foreach (var group in batchGroups)
            {
                int batchNumber = group.Key;

                _jobClient.Enqueue<RetryBatchJob>(
                    job => job.ExecuteAsync(broadcastId, batchNumber, CancellationToken.None));
            }

            return failedRecipients.Count;
        }
        public async Task<AudienceCountResponseDto> GetAudienceCountAsync(BroadcastAudience audience)
        {
            int count = await _broadcastRepository.GetAudienceCountAsync(audience);

            return new AudienceCountResponseDto
            {
                Audience = audience,
                TotalRecipients = count
            };
        }
    }
}