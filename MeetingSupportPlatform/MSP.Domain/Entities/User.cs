using Microsoft.AspNetCore.Identity;

namespace MSP.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public required string FullName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAtUtc { get; set; }
        public string? AvatarUrl { get; set; }
        public string? GoogleId { get; set; }
        public string? Provider { get; set; }
        public string? Organization { get; set; }
        public string? BusinessLicense { get; set; }
        public bool IsApproved { get; set; } = false; // Trạng thái duyệt cho BusinessOwner
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? ManagedById { get; set; }
        public virtual User? ManagedBy { get; set; }

        public virtual ICollection<User> ManagedUsers { get; set; } = new List<User>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
