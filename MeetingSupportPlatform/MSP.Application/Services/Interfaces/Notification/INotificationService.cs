using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Responses.Notification;

namespace MSP.Application.Services.Interfaces.Notification
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateInAppNotificationAsync(CreateNotificationRequest request);
        Task<NotificationResponse?> GetNotificationByIdAsync(Guid id);
        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId);
        Task<IEnumerable<NotificationResponse>> GetUserUnreadNotificationsAsync(Guid userId);
        Task<NotificationResponse> MarkAsReadAsync(Guid id);
        Task<bool> DeleteNotificationAsync(Guid id);
        Task<int> GetUnreadCountAsync(Guid userId);
        void SendEmailNotification(string toEmail, string title, string message);
    }
}
