namespace MSP.Application.Models.Requests.Notification
{
    public class BroadcastNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Data { get; set; }
    }
}
