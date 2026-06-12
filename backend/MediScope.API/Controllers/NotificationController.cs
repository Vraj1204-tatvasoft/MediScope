using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MediScope.Business.Services.Interfaces;

namespace MediScope.API.Controllers
{
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController
        : BaseController
    {
        private readonly
            INotificationService
            _notificationService;

        public NotificationController(
            INotificationService notificationService)
        {
            _notificationService =
                notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var result =
                await _notificationService
                    .GetMyNotificationsAsync(
                        CurrentUserId);

            return Success(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result =
                await _notificationService
                    .GetUnreadCountAsync(
                        CurrentUserId);

            return Success(result);
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService
                .MarkAllAsReadAsync(
                    CurrentUserId);

            return Success(true, "Notifications marked as read.");
        }
        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService
                .MarkAsReadAsync(id, CurrentUserId);

            return Success(true, "Notification marked as read.");
        }
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAll()
        {
            await _notificationService.ClearAllNotificationsAsync(CurrentUserId);

            return Success(true, "All notifications cleared successfully.");
        }
    }
}