using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using Microsoft.EntityFrameworkCore;
namespace MediScope.Data.Repositories
{
    public class BroadcastRepository : IBroadcastRepository
    {
        private readonly AppDbContext _context;

        public BroadcastRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateBroadcastAsync(Broadcast broadcast)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_create_broadcast(
                    {broadcast.Id},
                    {broadcast.Name},
                    {(int)broadcast.Channel},
                    {broadcast.Subject},
                    {broadcast.Message},
                    {(int)broadcast.Audience},
                    {(int)broadcast.Status},
                    {broadcast.BatchSize},
                    {broadcast.CreatedBy}
                )");
        }
        public async Task UpdateBroadcastAsync(Broadcast broadcast)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_update_broadcast(
                    {broadcast.Id},
                    {broadcast.Name},
                    {(int)broadcast.Channel},
                    {broadcast.Subject},
                    {broadcast.Message},
                    {(int)broadcast.Audience},
                    {(int)broadcast.Status}
                )");
        }
        public async Task SoftDeleteBroadcastAsync(Guid id)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_soft_delete_broadcast({id})
            ");
        }
        public async Task<Broadcast?> GetBroadcastByIdAsync(Guid id)
        {
            return await _context.Broadcasts
                .FromSqlInterpolated($@"SELECT * FROM fn_get_broadcast_by_id({id})")
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
        public async Task<BroadcastPagedResponseDto> GetBroadcastsPagedAsync(GetBroadcastsRequestDto request)
        {
            var dbResults = await _context.Database.SqlQuery<DbPagedBroadcast>($@"SELECT * FROM fn_get_broadcasts_paged(
            {request.Search},
            {request.Channel},
            {request.Status},
            {request.PageNumber},
            {request.PageSize}
        )").ToListAsync();
            var items = dbResults.Select(b => new BroadcastListItemDto
            {
                Id = b.Id,
                Name = b.Name,
                Channel = b.Channel,
                Subject = b.Subject,
                Audience = b.Audience,
                Status = b.Status,
                TotalRecipients = b.TotalRecipients,
                SentCount = b.SentCount,
                FailedCount = b.FailedCount,
                ScheduledAt = b.ScheduledAt,
                CompletedAt = b.CompletedAt,
                CreatedAt = b.CreatedAt
            }).ToList();

            return new BroadcastPagedResponseDto
            {
                Items = items,
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = dbResults.FirstOrDefault()?.TotalCount ?? 0
            };
        }
        public async Task MarkPendingAsync(Guid broadcastId, string hangfireJobId, int totalRecipients)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_mark_broadcast_pending(
                    {broadcastId},
                    {hangfireJobId},
                    {totalRecipients}
                )");
        }

        public async Task MarkProcessingAsync(Guid broadcastId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_mark_broadcast_processing({broadcastId})");
        }

        public async Task CompleteBroadcastAsync(Guid broadcastId, int sentCount, int failedCount, BroadcastStatus status, string? failureReason = null)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_complete_broadcast(
                    {broadcastId},
                    {sentCount},
                    {failedCount},
                    {(int)status},
                    {failureReason}
                )");
        }

        public async Task IncrementBroadcastCountsAsync(Guid broadcastId, int sentDelta, int failedDelta)
        {
            var (sentCount, failedCount) = await GetFinalCountsAsync(broadcastId);

            // Update the broadcast table with the absolute ground-truth counts
            await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE broadcasts SET sent_count = {sentCount}, 
                    failed_count = {failedCount}
                WHERE id = {broadcastId}");
        }
        public async Task SetRemainingBatchesAsync(Guid broadcastId, int totalBatches)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"CALL sp_set_remaining_batches({broadcastId}, {totalBatches})");
        }

        public async Task<int> DecrementRemainingBatchesAsync(Guid broadcastId, int sentDelta, int failedDelta)
        {
            var result = await _context.Database
                .SqlQuery<int>($@"SELECT fn_decrement_remaining_batches(
                        {broadcastId},
                        {sentDelta},
                        {failedDelta}
                    )")
                .ToListAsync();

            return result.FirstOrDefault();
        }
        // ── Audience helpers ──────────────────────────────────────────────────

        public async Task<List<DbAudienceMember>> GetAudienceContactsBatchAsync(BroadcastAudience audience, int offset, int limit)
        {
            return await _context.Database
                .SqlQuery<DbAudienceMember>($@"
                    SELECT * FROM fn_get_audience_contacts_batch(
                        {(int)audience},
                        {offset},
                        {limit}
                    )")
                .ToListAsync();
        }

        public async Task<int> GetAudienceCountAsync(BroadcastAudience audience)
        {
            var result = await _context.Database
                .SqlQuery<int>($@"
                    SELECT fn_get_audience_count({(int)audience})")
                .ToListAsync();

            return result.FirstOrDefault();
        }

        // ── Recipients ────────────────────────────────────────────────────────

        public async Task BulkInsertRecipientsAsync(Guid broadcastId, List<DbAudienceMember> contacts, int batchNumber)
        {
            var userIds = contacts.Select(c => c.UserId).ToArray();
            var emails = contacts.Select(c => c.Email).ToArray();
            var fullNames = contacts.Select(c => c.FullName).ToArray();
            var batchNumbers = contacts.Select(_ => batchNumber).ToArray();

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_bulk_insert_recipients(
                    {broadcastId},
                    {userIds},
                    {emails},
                    {fullNames},
                    {batchNumbers}
                )");
        }

        public async Task UpdateRecipientStatusAsync(Guid recipientId, RecipientStatus status, string? errorMessage = null)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_update_recipient_status(
                    {recipientId},
                    {(int)status},
                    {errorMessage}
                )");
        }

        public async Task<List<BroadcastRecipientRow>> GetRecipientsByBatchAsync(Guid broadcastId, int batchNumber)
        {
            return await _context.Database
                .SqlQuery<BroadcastRecipientRow>($@"SELECT * FROM fn_get_recipients_by_batch(
                        {broadcastId},
                        {batchNumber}
                    )")
                .ToListAsync();
        }

        public async Task<List<BroadcastRecipientRow>> GetFailedRecipientsAsync(Guid broadcastId)
        {
            return await _context.Database
                .SqlQuery<BroadcastRecipientRow>($@"
                    SELECT * FROM fn_get_failed_recipients({broadcastId})")
                .ToListAsync();
        }

        public async Task IncrementRetryCountAsync(Guid recipientId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                CALL sp_increment_retry_count({recipientId})");
        }


        public async Task<(int SentCount, int FailedCount)> GetFinalCountsAsync(Guid broadcastId)
        {
            var result = await _context.Database
                .SqlQuery<BroadcastFinalCountRow>($@"
                    SELECT * FROM fn_get_broadcast_final_counts({broadcastId})")
                .ToListAsync();

            var row = result.FirstOrDefault();
            return (row?.SentCount ?? 0, row?.FailedCount ?? 0);
        }

        public async Task<bool> UpdateBroadcastCountsAsync(Guid broadcastId, int sentDelta, int failedDelta)
        {
            var result = await _context.Database
                .SqlQuery<bool>($@"
                    SELECT fn_update_broadcast_counts(
                        {broadcastId},
                        {sentDelta},
                        {failedDelta}
                    )")
                .ToListAsync();

            return result.FirstOrDefault();
        }
    }
}