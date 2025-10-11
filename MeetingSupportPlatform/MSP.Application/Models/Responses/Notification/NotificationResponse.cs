namespace MSP.Application.Models.Responses.Notification
{
    public record NotificationResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Title { get; init; }
        public string Message { get; init; }
        public string? Type { get; init; }
        public string? EntityId { get; init; }
        public bool IsRead { get; init; }
        public DateTime? ReadAt { get; init; }
        public string? Data { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}

