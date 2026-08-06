using MediScope.Business.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MediScope.Common.Models.Enums;
namespace MediScope.Business.Services
{
    public class PushService : IPushService
    {
        private readonly ILogger<PushService> _logger;
        private readonly INotificationService _notificationService;

        public PushService(INotificationService notificationService, ILogger<PushService> logger)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string? Error)> SendAsync(Guid userId, string title, string body, CancellationToken ct = default)
        {
            try
            {
                string message = string.IsNullOrWhiteSpace(title)
                    ? body
                    : $"{title}: {body}";

                await _notificationService.CreateAsync(userId: userId, type: NotificationType.Info, message: message);

                _logger.LogInformation("Notification successfully dispatched to UserId: {UserId}", userId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push/Notification failed for UserId: {UserId}", userId);
                return (false, ex.Message);
            }
        }
    }
}