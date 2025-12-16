using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Responses.Notification;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Notification
{
    public interface INotificationService
    {
        Task<ApiResponse<NotificationResponse>> CreateInAppNotificationAsync(CreateNotificationRequest request);
        Task<ApiResponse<NotificationResponse?>> GetNotificationByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<NotificationResponse>>> GetUserNotificationsAsync(Guid userId);
        Task<ApiResponse<IEnumerable<NotificationResponse>>> GetUserUnreadNotificationsAsync(Guid userId);
        Task<ApiResponse<NotificationResponse>> MarkAsReadAsync(Guid id);
        Task<ApiResponse<string>> MarkAllAsReadAsync(Guid userId);
        Task<ApiResponse<bool>> DeleteNotificationAsync(Guid id);
        Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId);
        void SendEmailNotification(string toEmail, string title, string message);
        Task<ApiResponse<List<NotificationResponse>>> SendBulkNotificationAsync(SendBulkNotificationRequest request);

        Task<ApiResponse<string>> RegisterFCMTokenAsync(Guid userId, RegisterFCMTokenRequest request);
        Task<ApiResponse<string>> DeactivateFCMTokenAsync(string fcmToken);
    }
}
