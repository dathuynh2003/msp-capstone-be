using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Responses.Notification;

namespace MSP.Application.Services.Interfaces.Notification
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);
        Task<NotificationResponse?> GetNotificationByIdAsync(Guid id);
        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(string userId);
        Task<IEnumerable<NotificationResponse>> GetUserUnreadNotificationsAsync(string userId);
        Task<NotificationResponse> MarkAsReadAsync(Guid id);
        Task<bool> DeleteNotificationAsync(Guid id);
        Task<int> GetUnreadCountAsync(string userId);
        Task SendToUserAsync(string userId, string title, string message, string? type = null, string? data = null);
        //Task BroadcastNotificationAsync(string title, string message, string? type = null, string? data = null);
    }
}
