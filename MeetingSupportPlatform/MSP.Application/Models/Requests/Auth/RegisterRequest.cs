namespace MSP.Application.Models.Requests.Auth
{
    public record RegisterRequest
    {
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }

    }
}
