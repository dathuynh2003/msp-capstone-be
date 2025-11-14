namespace MSP.Application.Models.Requests.Notification
{
    public record CreateNotificationRequest
    {
        public required Guid UserId { get; init; }
        public Guid? ActorId { get; init; }
        public required string Title { get; init; }
        public required string Message { get; init; }
        public string? Type { get; init; }
        public string? EntityId { get; init; }
        public string? Data { get; init; }
    }
}

