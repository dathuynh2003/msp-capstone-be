using Microsoft.AspNetCore.Identity;

namespace MSP.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAtUtc { get; set; }
        public string? AvatarUrl { get; set; }
        public string? GoogleId { get; set; }
        public string? Provider { get; set; }
        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

    }
}
