using Hangfire;
using MSP.Shared.Enums;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Abstracts;
using MSP.Application.Models.Responses.Notification;
using MSP.Application.Models.Requests.Notification;
using MSP.Shared.Common;
using Microsoft.AspNetCore.Identity;
using MSP.Domain.Entities;
using MSP.Application.Repositories;
using MSP.Shared.Specifications;

namespace MSP.Application.Services.Implementations.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly ISignalRNotificationService _signalRService;
        private readonly UserManager<User> _userManager;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IFCMNotificationService _fcmService;

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailSender emailSender,
            ISignalRNotificationService signalRService,
            UserManager<User> userManager,
            IProjectRepository projectRepository,
            IProjectMemberRepository projectMemberRepository,
            IUserDeviceRepository userDeviceRepository,
            IFCMNotificationService fcmService)
        {
            _notificationRepository = notificationRepository;
            _emailSender = emailSender;
            _signalRService = signalRService;
            _userManager = userManager;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _userDeviceRepository = userDeviceRepository;
            _fcmService = fcmService;
        }

        /// Send email notification via Hangfire
        public void SendEmailNotification(string toEmail, string title, string message)
        {
            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(toEmail, title, message, null, null, null, null, true)
            );
        }

        /// Create InApp notification and push via SignalR
        public async Task<ApiResponse<NotificationResponse>> CreateInAppNotificationAsync(CreateNotificationRequest request)
        {
            try
            {
                var notification = new Domain.Entities.Notification
                {
                    UserId = request.UserId,
                    ActorId = request.ActorId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type ?? NotificationTypeEnum.InApp.ToString(),
                    EntityId = request.EntityId,
                    Data = request.Data,
                    CreatedAt = DateTime.UtcNow
                };

                var createdNotification = await _notificationRepository.CreateAsync(notification);
                var response = MapToResponse(createdNotification);

                // Push notification via SignalR in real-time
                await _signalRService.SendNotificationToUserAsync(request.UserId, response);

                // Gửi FCM notification (cho app background/offline)
                try
                {
                    var fcmData = new Dictionary<string, string>
                    {
                        { "notificationId", response.Id.ToString() },
                        { "type", response.Type ?? "InApp" },
                        { "entityId", request.EntityId ?? "" },
                        { "entityType", DetermineEntityType(response.Type) },
                        { "title", request.Title },
                        { "message", request.Message },
                        { "click_action", "FLUTTER_NOTIFICATION_CLICK" }
                    };
                    await _fcmService.SendNotificationToUserAsync(request.UserId, request.Title, request.Message, fcmData);
                }
                catch (Exception fcmEx)
                {
                    // Log FCM errors but do not fail the main notification flow
                    Console.WriteLine($"[FCM] Failed to send push notification: {fcmEx.Message}");
                }
                // Update unread count
                var unreadCount = await _notificationRepository.GetUnreadCountAsync(request.UserId);
                await _signalRService.UpdateUnreadCountAsync(request.UserId, unreadCount);

                return ApiResponse<NotificationResponse>.SuccessResponse(response, "Notification sent successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationResponse>.ErrorResponse(null, $"Error creating notification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationResponse?>> GetNotificationByIdAsync(Guid id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return ApiResponse<NotificationResponse?>.ErrorResponse(null, "Notification not found");
                }

                var response = MapToResponse(notification);
                return ApiResponse<NotificationResponse?>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationResponse?>.ErrorResponse(null, $"Error retrieving notification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<NotificationResponse>>> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId);
                var response = notifications.Select(MapToResponse).ToList();

                return ApiResponse<IEnumerable<NotificationResponse>>.SuccessResponse(
                    response,
                    $"Retrieved {response.Count} notifications"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<NotificationResponse>>.ErrorResponse(
                    Enumerable.Empty<NotificationResponse>(),
                    $"Error retrieving notifications: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<NotificationResponse>>> GetUserUnreadNotificationsAsync(Guid userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
                var response = notifications.Select(MapToResponse).ToList();

                return ApiResponse<IEnumerable<NotificationResponse>>.SuccessResponse(
                    response,
                    $"Retrieved {response.Count} unread notifications"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<NotificationResponse>>.ErrorResponse(
                    Enumerable.Empty<NotificationResponse>(),
                    $"Error retrieving unread notifications: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<NotificationResponse>> MarkAsReadAsync(Guid id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return ApiResponse<NotificationResponse>.ErrorResponse(null, "Notification not found");
                }

                if (notification.IsRead)
                {
                    var existingResponse = MapToResponse(notification);
                    return ApiResponse<NotificationResponse>.SuccessResponse(
                        existingResponse,
                        "Notification already marked as read"
                    );
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                var updatedNotification = await _notificationRepository.UpdateAsync(notification);
                var response = MapToResponse(updatedNotification);

                // Update unread count via SignalR
                var unreadCount = await _notificationRepository.GetUnreadCountAsync(notification.UserId);
                await _signalRService.UpdateUnreadCountAsync(notification.UserId, unreadCount);

                return ApiResponse<NotificationResponse>.SuccessResponse(response, "Notification marked as read");
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationResponse>.ErrorResponse(null, $"Error marking notification as read: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                var unreadNotifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);

                if (!unreadNotifications.Any())
                {
                    return ApiResponse<string>.SuccessResponse("No unread notifications to mark");
                }

                int markedCount = 0;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _notificationRepository.UpdateAsync(notification);
                    markedCount++;
                }

                // Update unread count via SignalR
                await _signalRService.UpdateUnreadCountAsync(userId, 0);

                return ApiResponse<string>.SuccessResponse($"{markedCount} notifications marked as read");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse(null, $"Error marking all notifications as read: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteNotificationAsync(Guid id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return ApiResponse<bool>.ErrorResponse(false, "Notification not found");
                }

                var result = await _notificationRepository.DeleteAsync(id);

                if (result)
                {
                    // Update unread count if deleted notification was unread
                    if (!notification.IsRead)
                    {
                        var unreadCount = await _notificationRepository.GetUnreadCountAsync(notification.UserId);
                        await _signalRService.UpdateUnreadCountAsync(notification.UserId, unreadCount);
                    }

                    return ApiResponse<bool>.SuccessResponse(true, "Notification deleted successfully");
                }

                return ApiResponse<bool>.ErrorResponse(false, "Failed to delete notification");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse(false, $"Error deleting notification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                return ApiResponse<int>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<int>.ErrorResponse(0, $"Error retrieving unread count: {ex.Message}");
            }
        }

        /// <summary>
        /// PM sends bulk notification to team members (project or specific users)
        /// </summary>
        public async Task<ApiResponse<List<NotificationResponse>>> SendBulkNotificationAsync(SendBulkNotificationRequest request)
        {
            try
            {
                // 1. Validate sender authorization
                var sender = await _userManager.FindByIdAsync(request.SenderId.ToString());
                if (sender == null)
                {
                    return ApiResponse<List<NotificationResponse>>.ErrorResponse(null, "Sender not found");
                }

                // 2. Determine recipient list
                List<Guid> recipientIds;

                if (request.ProjectId.HasValue)
                {
                    // Send to all active project members
                    var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value);
                    if (project == null || project.IsDeleted)
                    {
                        return ApiResponse<List<NotificationResponse>>.ErrorResponse(null, "Project not found");
                    }

                    var projectMembers = await _projectMemberRepository.GetProjectMembersByProjectIdAsync(request.ProjectId.Value);
                    recipientIds = projectMembers
                        .Where(pm => pm.LeftAt == null) // Only active members
                        .Select(pm => pm.MemberId)
                        .ToList();

                    if (!recipientIds.Any())
                    {
                        return ApiResponse<List<NotificationResponse>>.ErrorResponse(null, "No active members found in project");
                    }
                }
                else if (request.RecipientIds != null && request.RecipientIds.Any())
                {
                    recipientIds = request.RecipientIds;
                }
                else
                {
                    return ApiResponse<List<NotificationResponse>>.ErrorResponse(
                        null,
                        "Either ProjectId or RecipientIds must be provided"
                    );
                }

                // 3. Create and send notifications to all recipients
                var responses = new List<NotificationResponse>();

                foreach (var recipientId in recipientIds)
                {
                    var notification = new Domain.Entities.Notification
                    {
                        UserId = recipientId,
                        ActorId = request.SenderId,
                        Title = request.Title,
                        Message = request.Message,
                        Type = request.NotificationType,
                        EntityId = request.EntityId,
                        Data = request.Data,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    var created = await _notificationRepository.CreateAsync(notification);
                    var response = MapToResponse(created);
                    responses.Add(response);

                    // Push realtime via SignalR
                    await _signalRService.SendNotificationToUserAsync(recipientId, response);

                    // Gửi FCM notification
                    try
                    {
                        var fcmData = new Dictionary<string, string>
                        {
                            { "notificationId", response.Id.ToString() },
                            { "type", response.Type ?? "InApp" },
                            { "entityId", request.EntityId ?? "" },
                            { "entityType", DetermineEntityType(response.Type) },
                            { "title", request.Title },
                            { "message", request.Message },
                            { "click_action", "FLUTTER_NOTIFICATION_CLICK" }
                        };
                        await _fcmService.SendNotificationToUserAsync(
                            recipientId,
                            request.Title,
                            request.Message,
                            fcmData);
                    }
                    catch (Exception fcmEx)
                    {
                        Console.WriteLine($"[FCM] Failed for user {recipientId}: {fcmEx.Message}");
                    }

                    // Update unread count
                    var unreadCount = await _notificationRepository.GetUnreadCountAsync(recipientId);
                    await _signalRService.UpdateUnreadCountAsync(recipientId, unreadCount);
                }

                return ApiResponse<List<NotificationResponse>>.SuccessResponse(
                    responses,
                    $"Successfully sent notification to {responses.Count} recipient(s)"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<NotificationResponse>>.ErrorResponse(
                    null,
                    $"Error sending bulk notification: {ex.Message}"
                );
            }
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
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt,
                Data = notification.Data
            };
        }

        /// <summary>
        /// Register FCM token for push notifications
        /// </summary>
        public async Task<ApiResponse<string>> RegisterFCMTokenAsync(Guid userId, RegisterFCMTokenRequest request)
        {
            try
            {
                // Validate user exists
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    //Console.WriteLine("❌ [FCM] User not found: {UserId}", userId);
                    return ApiResponse<string>.ErrorResponse(null, "User not found");
                }

                // Check if token already exists
                var existingDevice = await _userDeviceRepository.GetByFCMTokenAsync(request.FCMToken);

                if (existingDevice != null)
                {
                    // Update existing device
                    existingDevice.UserId = userId;
                    existingDevice.Platform = request.Platform;
                    existingDevice.DeviceId = request.DeviceId;
                    existingDevice.DeviceName = request.DeviceName;
                    existingDevice.IsActive = true;
                    existingDevice.LastActiveAt = DateTime.UtcNow;
                    existingDevice.UpdatedAt = DateTime.UtcNow;

                    await _userDeviceRepository.UpdateAsync(existingDevice);

                    //Console.WriteLine("✅ [FCM] Updated token for user {UserId}", userId);
                    return ApiResponse<string>.SuccessResponse(
                        "FCM token updated successfully",
                        "Token registered and updated");
                }
                else
                {
                    // Create new device
                    var newDevice = new UserDevice
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        FCMToken = request.FCMToken,
                        Platform = request.Platform,
                        DeviceId = request.DeviceId,
                        DeviceName = request.DeviceName,
                        IsActive = true,
                        LastActiveAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _userDeviceRepository.AddAsync(newDevice);
                    await _userDeviceRepository.SaveChangesAsync();

                    //Console.WriteLine("✅ [FCM] Registered new token for user {UserId}", userId);
                    return ApiResponse<string>.SuccessResponse(
                        "FCM token registered successfully",
                        "New token registered");
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex, "❌ [FCM] Error registering token for user {UserId}", userId);
                return ApiResponse<string>.ErrorResponse(
                    null,
                    $"Error registering FCM token: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivate FCM token (when user logout)
        /// </summary>
        public async Task<ApiResponse<string>> DeactivateFCMTokenAsync(string fcmToken)
        {
            try
            {
                var device = await _userDeviceRepository.GetByFCMTokenAsync(fcmToken);

                if (device == null)
                {
                    //Console.WriteLine("⚠️ [FCM] Device not found for token");
                    return ApiResponse<string>.ErrorResponse(null, "Device not found");
                }

                var success = await _userDeviceRepository.DeactivateDeviceAsync(device.Id);

                if (success)
                {
                    //Console.WriteLine("✅ [FCM] Deactivated token for user {UserId}", device.UserId);
                    return ApiResponse<string>.SuccessResponse(
                        "FCM token deactivated successfully",
                        "Token deactivated");
                }

                return ApiResponse<string>.ErrorResponse(null, "Failed to deactivate token");
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex, "❌ [FCM] Error deactivating token");
                return ApiResponse<string>.ErrorResponse(
                    null,
                    $"Error deactivating token: {ex.Message}");
            }
        }

        private string DetermineEntityType(string? notificationType)
        {
            return notificationType switch
            {
                "TaskAssignment" => "task",
                "TaskUpdate" => "task",
                "ProjectUpdate" => "project",
                "MeetingReminder" => "meeting",
                _ => "notification"
            };
        }
    }
}
