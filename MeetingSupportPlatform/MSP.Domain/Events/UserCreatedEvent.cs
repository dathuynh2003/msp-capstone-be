namespace MSP.Domain.Events
{
    public record UserCreatedEvent : IntegrationEvent
    {
        public required Guid UserId { get; init; }
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Role { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public string? ConfirmationToken { get; init; }
    }
}

