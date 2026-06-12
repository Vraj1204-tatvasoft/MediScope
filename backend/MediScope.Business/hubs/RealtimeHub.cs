using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims; // 🛠️ ADD THIS IMPORT

namespace MediScope.Business.Hubs
{
    [Authorize]
    public class RealtimeHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // 🛠️ FIX: Fall back to standard ClaimTypes scheme if short string payload reads null
            var userId = Context.User?.FindFirst("id")?.Value
                         ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                // Joins connection to a named channel group matching their UserId string
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"[SIGNALR SUCCESS] RealtimeHub Group Subscribed for User: {userId}");
            }
            else
            {
                Console.WriteLine("[SIGNALR WARNING] Connection passed authorization checks, but UserId claim parsing returned null.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("id")?.Value
                         ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                Console.WriteLine($"RealtimeHub Disconnected: {userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}