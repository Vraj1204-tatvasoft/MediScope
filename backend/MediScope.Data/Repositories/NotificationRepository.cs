using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;

namespace MediScope.Data.Repositories
{
    public class NotificationRepository
        : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
            => await _dbSet
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(Guid userId)
            => await _dbSet
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }
        }

        public async Task<bool> HasTodayReminderAsync(Guid userId)
        {
            var todayStart = DateTime.UtcNow.Date;
            return await _dbSet
                .AnyAsync(n =>
                    n.UserId == userId &&
                    n.Type == NotificationType.Reminder &&
                    n.CreatedAt >= todayStart &&
                    !n.IsDeleted);
        }
        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(n =>
                    n.Id == id &&
                    !n.IsDeleted);
        }
        public async Task ClearAllAsync(Guid userId)
        {
            var notifications =
                await _dbSet
                    .Where(n =>
                        n.UserId == userId
                        && !n.IsDeleted)
                    .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsDeleted = true;

                notification.DeletedAt =
                    DateTime.UtcNow;
            }
        }
    }
}