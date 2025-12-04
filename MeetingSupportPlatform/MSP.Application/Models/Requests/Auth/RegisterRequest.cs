namespace MSP.Application.Models.Requests.Auth
{
    public record RegisterRequest
    {
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string PhoneNumber { get; init; }
        public required string Role { get; init; } // Bắt buộc phải có role
        public string? Organization { get; set; }
        public string? BusinessLicense { get; set; }
        public string? InviteToken { get; set; }
    }
}
