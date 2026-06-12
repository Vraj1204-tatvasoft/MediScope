using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);

        Task<int> GetUnreadCountAsync(Guid userId);

        Task MarkAllAsReadAsync(Guid userId);

        Task<bool> HasTodayReminderAsync(Guid userId);
        Task ClearAllAsync(Guid userId);
        Task<Notification?> GetByIdAsync(Guid id);
    }
}