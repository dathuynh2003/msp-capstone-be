using Microsoft.AspNetCore.SignalR;
using MSP.Application.Models.Responses.Notification;
using MSP.Application.Services.Interfaces.Notification;
using Microsoft.Extensions.Logging;

namespace MSP.Infrastructure.Services
{
    /// <summary>
    /// Implementation of SignalR notification service
    /// Handles real-time notification broadcasting
    /// Uses generic Hub type to avoid circular dependency
    /// </summary>
    public class SignalRNotificationService<THub> : ISignalRNotificationService where THub : Hub
    {
        private readonly IHubContext<THub> _hubContext;
        private readonly ILogger<SignalRNotificationService<THub>> _logger;

        public SignalRNotificationService(
            IHubContext<THub> hubContext,
            ILogger<SignalRNotificationService<THub>> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger;
        }

        /// <summary>
        /// Send notification to a specific user
        /// </summary>
        public async Task SendNotificationToUserAsync(Guid userId, NotificationResponse notification)
        {
            try
            {
                var groupName = $"user_{userId}";
                
                await _hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation(
                    "Sent notification {NotificationId} to user {UserId} via SignalR",
                    notification.Id,
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send notification {NotificationId} to user {UserId} via SignalR",
                    notification.Id,
                    userId);
            }
        }

        /// <summary>
        /// Send notification to multiple users
        /// </summary>
        public async Task SendNotificationToUsersAsync(IEnumerable<Guid> userIds, NotificationResponse notification)
        {
            var tasks = userIds.Select(userId => SendNotificationToUserAsync(userId, notification));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Send notification to a group
        /// </summary>
        public async Task SendNotificationToGroupAsync(string groupName, NotificationResponse notification)
        {
            try
            {
                await _hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation(
                    "Sent notification {NotificationId} to group {GroupName} via SignalR",
                    notification.Id,
                    groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send notification {NotificationId} to group {GroupName} via SignalR",
                    notification.Id,
                    groupName);
            }
        }

        /// <summary>
        /// Broadcast notification to all connected users
        /// </summary>
        public async Task BroadcastNotificationAsync(NotificationResponse notification)
        {
            try
            {
                await _hubContext.Clients.All
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation(
                    "Broadcasted notification {NotificationId} to all users via SignalR",
                    notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to broadcast notification {NotificationId} via SignalR",
                    notification.Id);
            }
        }

        /// <summary>
        /// Update unread count for a user
        /// </summary>
        public async Task UpdateUnreadCountAsync(Guid userId, int unreadCount)
        {
            try
            {
                var groupName = $"user_{userId}";
                
                await _hubContext.Clients.Group(groupName)
                    .SendAsync("UpdateUnreadCount", unreadCount);

                _logger.LogInformation(
                    "Updated unread count {Count} for user {UserId} via SignalR",
                    unreadCount,
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update unread count for user {UserId} via SignalR",
                    userId);
            }
        }
    }
}
