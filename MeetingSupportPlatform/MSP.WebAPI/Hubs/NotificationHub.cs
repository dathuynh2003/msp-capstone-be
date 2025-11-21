using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MSP.WebAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// Handles client connections and message broadcasting
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when a client connects to the hub
        /// Adds user to their personal group for targeted notifications
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? Context.User?.FindFirst("userId")?.Value
                      ?? Context.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                _logger.LogInformation(
                    "User {UserId} connected to NotificationHub with ConnectionId {ConnectionId}",
                    userId,
                    Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning(
                    "Anonymous user connected with ConnectionId {ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? Context.User?.FindFirst("userId")?.Value
                      ?? Context.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation(
                    "User {UserId} disconnected from NotificationHub with ConnectionId {ConnectionId}",
                    userId,
                    Context.ConnectionId);
            }

            if (exception != null)
            {
                _logger.LogError(exception, 
                    "User disconnected with error. ConnectionId: {ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client can manually join a group (e.g., project group, organization group)
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation(
                "ConnectionId {ConnectionId} joined group {GroupName}",
                Context.ConnectionId,
                groupName);
        }

        /// <summary>
        /// Client can manually leave a group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation(
                "ConnectionId {ConnectionId} left group {GroupName}",
                Context.ConnectionId,
                groupName);
        }

        /// <summary>
        /// Mark notification as read (client-initiated)
        /// </summary>
        public async Task MarkNotificationAsRead(Guid notificationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? Context.User?.FindFirst("userId")?.Value
                      ?? Context.User?.FindFirst("sub")?.Value;

            _logger.LogInformation(
                "User {UserId} marked notification {NotificationId} as read",
                userId,
                notificationId);

            // Broadcast to other devices of the same user
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.OthersInGroup($"user_{userId}")
                    .SendAsync("NotificationRead", notificationId);
            }
        }
    }
}
