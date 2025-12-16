namespace MSP.Application.Services.Interfaces.Notification
{
    public interface IFCMNotificationService
    {
        Task<bool> SendNotificationToUserAsync(
            Guid userId,
            string title,
            string body,
            Dictionary<string, string>? data = null);

        Task<bool> SendNotificationToDeviceAsync(
            string fcmToken,
            string title,
            string body,
            Dictionary<string, string>? data = null);

        Task<int> SendBulkNotificationAsync(
            List<Guid> userIds,
            string title,
            string body,
            Dictionary<string, string>? data = null);
    }
}
