using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Enums;
namespace MediScope.Business.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>>
            GetMyNotificationsAsync(Guid userId);

        Task<int>
            GetUnreadCountAsync(Guid userId);

        Task MarkAllAsReadAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task CreateAsync(Guid userId, NotificationType type, string message);
        Task ClearAllNotificationsAsync(Guid userId);
    }
}