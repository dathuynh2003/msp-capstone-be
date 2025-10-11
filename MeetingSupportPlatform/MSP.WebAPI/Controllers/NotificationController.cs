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

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<IActionResult> GetUserUnreadNotifications(Guid userId)
        {
            var notifications = await _notificationService.GetUserUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(Guid userId)
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { UnreadCount = count });
        }

        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return Ok(new { Deleted = result });
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationRequest request)
        {
            await _notificationService.CreateInAppNotificationAsync(request);
            
            return Ok(new { Message = "Notification sent successfully" });
        }

        //[HttpPost("send-realtime")]
        //public async Task<IActionResult> SendRealtimeNotification([FromBody] CreateNotificationRequest request)
        //{
        //    var result = await _notificationService.CreateInAppNotificationAsync(request);
        //    return Ok(new { Message = "Real-time notification sent successfully", Notification = result });
        //}

        //[HttpPost("broadcast")]
        //public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationRequest request)
        //{
        //    await _notificationService.BroadcastNotificationAsync(request.Title, request.Message, request.Type, request.Data);
        //    return Ok(new { Message = "Broadcast notification sent successfully" });
        //}
    }
}

