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

        public async Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
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

            // Send email notification if type is Email
            if (request.Type == NotificationTypeEnum.Email.ToString())
            {
                try
                {
                    var email = ExtractEmailFromData(request.Data) ?? request.UserId;

                    await _emailSender.SendEmailAsync(
                        email,
                        request.Title,
                        request.Message,
                        isHtml: true
                    );
                }
                catch (Exception ex)
                {
                }
            }

            return MapToResponse(createdNotification);
        }

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

        public async Task SendToUserAsync(string userId, string title, string message, string? type = null, string? data = null)
        {
            // If type is Email and userId looks like an email, send email directly
            if (type == "Email" && userId.Contains("@"))
            {
                try
                {
                    await _emailSender.SendEmailAsync(
                        userId, // userId is actually email in this case
                        title,
                        message,
                        isHtml: true
                    );
                }
                catch (Exception ex)
                {
                    // Log error but don't fail
                    // In production, use proper logging
                }
            }
            else
            {
                // For in-app notifications, store in database
                var request = new CreateNotificationRequest
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Data = data
                };

                await CreateNotificationAsync(request);
            }
        }

        //public async Task BroadcastNotificationAsync(string title, string message, string? type = null, string? data = null)
        //{
        //    // Send real-time notification to all connected users
        //    await _signalRNotificationService.SendToAllAsync(new
        //    {
        //        Type = "BroadcastNotification",
        //        Title = title,
        //        Message = message,
        //        NotificationType = type ?? "InApp",
        //        Data = data,
        //        Timestamp = DateTime.UtcNow
        //    });
        //}

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

        private static string? ExtractEmailFromData(string? data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            try
            {
                // Try to parse JSON data to extract email
                // This is a simple implementation - you might want to use a proper JSON parser
                if (data.Contains("\"email\""))
                {
                    // Extract email from JSON-like string
                    var emailStart = data.IndexOf("\"email\"") + 8;
                    var emailEnd = data.IndexOf("\"", emailStart);
                    if (emailEnd > emailStart)
                    {
                        return data.Substring(emailStart, emailEnd - emailStart);
                    }
                }
            }
            catch
            {
                // If parsing fails, return null
            }

            return null;
        }
    }
}
