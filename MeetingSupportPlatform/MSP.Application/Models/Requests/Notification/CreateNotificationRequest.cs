namespace MSP.Application.Models.Requests.Notification
{
    public record CreateNotificationRequest
    {
        public required string UserId { get; init; }
        public required string Title { get; init; }
        public required string Message { get; init; }
        public string? Type { get; init; }
        public string? Data { get; init; }
    }
}

