using Microsoft.AspNetCore.SignalR;

using MediScope.Business.Hubs;
using MediScope.Business.Services.Interfaces;

using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;

using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class NotificationService
        : INotificationService
    {
        private readonly IUnitOfWork _uow;

        private readonly
            IHubContext<RealtimeHub>
            _hub;

        public NotificationService(
            IUnitOfWork uow,
            IHubContext<RealtimeHub> hub)
        {
            _uow = uow;
            _hub = hub;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetMyNotificationsAsync(Guid userId)
        {
            var items =
                await _uow.Notifications
                    .GetByUserIdAsync(userId);

            return items.Select(Map);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _uow.Notifications
                .GetUnreadCountAsync(userId);
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _uow.Notifications
                .MarkAllAsReadAsync(userId);

            await _uow.SaveChangesAsync();
        }
        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification =
                await _uow.Notifications
                    .GetByIdAsync(notificationId)

                ?? throw new Exception(
                    "Notification not found.");

            // SECURITY CHECK

            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Unauthorized notification access.");
            }

            // ALREADY READ

            if (notification.IsRead)
                return;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            _uow.Notifications.Update(notification);

            await _uow.SaveChangesAsync();

            // OPTIONAL LIVE UPDATE

            await _hub.Clients
                .Group(userId.ToString())
                .SendAsync(
                    "NotificationRead",
                    notificationId);
        }
        public async Task CreateAsync(
            Guid userId,
            string type,
            string message)
        {
            var entity =
                new Notification
                {
                    UserId = userId,
                    Type = type,
                    Message = message
                };

            await _uow.Notifications
                .AddAsync(entity);

            await _uow.SaveChangesAsync();

            var dto = Map(entity);

            // LIVE PUSH

            await _hub.Clients
                .Group(userId.ToString())
                .SendAsync(
                    "NotificationRecieved",
                    dto);
        }
        public async Task ClearAllNotificationsAsync(Guid userId)
        {
            await _uow.Notifications
                .ClearAllAsync(userId);

            await _uow.SaveChangesAsync();

            // OPTIONAL REALTIME PUSH

            await _hub.Clients
                .Group(userId.ToString())
                .SendAsync(
                    "NotificationsCleared");
        }
        private static NotificationResponseDto
            Map(Notification n)
        {
            return new NotificationResponseDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            };
        }
    }
}