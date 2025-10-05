namespace MSP.Application.Models.Responses.Notification
{
    public record NotificationResponse
    {
        public Guid Id { get; init; }
        public string UserId { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? Type { get; init; }
        public bool IsRead { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ReadAt { get; init; }
        public string? Data { get; init; }
    }
}

