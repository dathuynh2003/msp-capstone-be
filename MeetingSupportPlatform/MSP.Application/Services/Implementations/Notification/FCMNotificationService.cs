using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;

namespace MSP.Application.Services.Implementations.Notification
{
    public class FCMNotificationService : IFCMNotificationService
    {
        private readonly ILogger<FCMNotificationService> _logger;
        private readonly IUserDeviceRepository _deviceRepository;

        public FCMNotificationService(
            ILogger<FCMNotificationService> logger,
            IUserDeviceRepository deviceRepository)
        {
            _logger = logger;
            _deviceRepository = deviceRepository;
        }

        /// <summary>
        /// Send notification to all active devices of a user
        /// </summary>
        public async Task<bool> SendNotificationToUserAsync(
            Guid userId,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            try
            {
                // Get all active devices của user
                var devices = await _deviceRepository.GetActiveDevicesByUserIdAsync(userId);

                if (!devices.Any())
                {
                    _logger.LogWarning("⚠️ [FCM] No active devices found for user {UserId}", userId);
                    return false;
                }

                var tokens = devices.Select(d => d.FCMToken).ToList();

                // Send to multiple devices
                return await SendToMultipleDevicesAsync(tokens, title, body, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [FCM] Error sending notification to user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Send notification to a specific device by FCM token
        /// </summary>
        public async Task<bool> SendNotificationToDeviceAsync(
            string fcmToken,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message
                {
                    Token = fcmToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            ChannelId = "msp_notifications",
                            Sound = "default",
                            Priority = NotificationPriority.HIGH
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "default",
                            Badge = 1,
                            ContentAvailable = true
                        }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                _logger.LogInformation("✅ [FCM] Sent successfully. MessageId: {MessageId}", response);
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, "❌ [FCM] Firebase error: {ErrorCode}", ex.ErrorCode);

                // Handle invalid token
                //if (ex.ErrorCode == MessagingErrorCode.Unregistered ||
                //    ex.ErrorCode == MessagingErrorCode.InvalidArgument)
                //{
                //    _logger.LogWarning("⚠️ [FCM] Invalid/unregistered token, should deactivate");
                //    // TODO: Deactivate token in database
                //}

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [FCM] Error sending notification");
                return false;
            }
        }

        /// <summary>
        /// Send bulk notifications to multiple users
        /// </summary>
        public async Task<int> SendBulkNotificationAsync(
            List<Guid> userIds,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            int successCount = 0;

            foreach (var userId in userIds)
            {
                var sent = await SendNotificationToUserAsync(userId, title, body, data);
                if (sent) successCount++;
            }

            _logger.LogInformation("✅ [FCM] Bulk send completed: {SuccessCount}/{TotalCount}",
                successCount, userIds.Count);

            return successCount;
        }

        /// <summary>
        /// Send to multiple device tokens (multicast)
        /// </summary>
        private async Task<bool> SendToMultipleDevicesAsync(
            List<string> tokens,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new MulticastMessage
                {
                    Tokens = tokens,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            ChannelId = "msp_notifications",
                            Sound = "default",
                            Priority = NotificationPriority.HIGH
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                _logger.LogInformation(
                    "✅ [FCM] Multicast sent: {SuccessCount}/{TotalCount} successful",
                    response.SuccessCount, tokens.Count);

                // Log failed tokens
                if (response.FailureCount > 0)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            _logger.LogWarning(
                                "⚠️ [FCM] Failed to send to token {TokenIndex}: {Error}",
                                i, response.Responses[i].Exception?.Message);
                        }
                    }
                }

                return response.SuccessCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [FCM] Error in multicast send");
                return false;
            }
        }
    }
}
