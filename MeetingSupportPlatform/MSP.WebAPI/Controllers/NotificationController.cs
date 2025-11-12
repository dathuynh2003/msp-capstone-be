using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Services.Interfaces.Notification;

namespace NotificationService.API.Controllers
{
    [ApiController]
    [Route("api/v1/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all notifications for a user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            var response = await _notificationService.GetUserNotificationsAsync(userId);
            if (!response.Success)
            {
                _logger.LogError("GetUserNotifications failed for userId {UserId}: {Message}", userId, response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// Get unread notifications for a user
        /// </summary>
        [HttpGet("user/{userId}/unread")]
        public async Task<IActionResult> GetUserUnreadNotifications(Guid userId)
        {
            var response = await _notificationService.GetUserUnreadNotificationsAsync(userId);
            if (!response.Success)
            {
                _logger.LogError("GetUserUnreadNotifications failed for userId {UserId}: {Message}", userId, response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// Get unread notification count for a user
        /// </summary>
        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(Guid userId)
        {
            var response = await _notificationService.GetUnreadCountAsync(userId);
            if (!response.Success)
            {
                _logger.LogError("GetUnreadCount failed for userId {UserId}: {Message}", userId, response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var response = await _notificationService.MarkAsReadAsync(id);
            if (!response.Success)
            {
                _logger.LogWarning("MarkAsRead failed for notificationId {Id}: {Message}", id, response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead(Guid userId)
        {
            var response = await _notificationService.MarkAllAsReadAsync(userId);
            if (!response.Success)
            {
                _logger.LogError("MarkAllAsRead failed for userId {UserId}: {Message}", userId, response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var response = await _notificationService.DeleteNotificationAsync(id);
            if (!response.Success)
            {
                _logger.LogError("DeleteNotification failed for notificationId {Id}: {Message}", id, response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// Send a new notification (single user)
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationRequest request)
        {
            var response = await _notificationService.CreateInAppNotificationAsync(request);
            if (!response.Success)
            {
                _logger.LogError("SendNotification failed for userId {UserId}: {Message}", request.UserId, response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// PM sends bulk notification to project members or specific users
        /// Authorization: Only PM, BO, or Admin
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "ProjectManager,BusinessOwner,Admin")]
        public async Task<IActionResult> SendBulkNotification([FromBody] SendBulkNotificationRequest request)
        {
            var response = await _notificationService.SendBulkNotificationAsync(request);
            if (!response.Success)
            {
                _logger.LogError("SendBulkNotification failed: {Message}", response.Message);
            }
            return Ok(response);
        }
    }
}

