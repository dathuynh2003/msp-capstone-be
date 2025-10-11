using Hangfire;
using MSP.Shared.Enums;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Abstracts;
using MSP.Application.Models.Responses.Notification;
using MSP.Application.Models.Requests.Notification;

namespace MSP.Application.Services.Implementations.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailSender _emailSender;

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailSender emailSender)
        {
            _notificationRepository = notificationRepository;
            _emailSender = emailSender;
        }

        /// G?i email notification qua Hangfire
        public void SendEmailNotification(string toEmail, string title, string message)
        {
            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(toEmail, title, message, null, null, null, null, true)
            );
        }

        /// T?o notification InApp (tách riêng)
        public async Task<NotificationResponse> CreateInAppNotificationAsync(CreateNotificationRequest request)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type ?? NotificationTypeEnum.InApp.ToString(),
                Data = request.Data,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            return MapToResponse(createdNotification);
        }

        // Các hàm CRUD notification gi? nguyên, code rõ ràng:
        public async Task<NotificationResponse?> GetNotificationByIdAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            return notification != null ? MapToResponse(notification) : null;
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return notifications.Select(MapToResponse);
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserUnreadNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
            return notifications.Select(MapToResponse);
        }

        public async Task<NotificationResponse> MarkAsReadAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                throw new ArgumentException("Notification not found");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            var updatedNotification = await _notificationRepository.UpdateAsync(notification);

            return MapToResponse(updatedNotification);
        }

        public async Task<bool> DeleteNotificationAsync(Guid id)
        {
            return await _notificationRepository.DeleteAsync(id);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        // Helper mapping
        private static NotificationResponse MapToResponse(Domain.Entities.Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                Data = notification.Data
            };
        }
    }
}
