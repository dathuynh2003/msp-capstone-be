namespace MSP.Application.Models.Requests.Notification
{
    /// <summary>
    /// PM sends notification to project members or specific users
    /// </summary>
    public record SendBulkNotificationRequest
    {
        public required Guid SenderId { get; init; }
        public Guid? ProjectId { get; init; }
        public List<Guid>? RecipientIds { get; init; }
        public required string Title { get; init; }
        public required string Message { get; init; }
        public string NotificationType { get; init; } = "TeamNotification";
        public string? EntityId { get; init; }
        public string? Data { get; init; }
    }
}
