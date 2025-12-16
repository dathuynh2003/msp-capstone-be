using System.ComponentModel.DataAnnotations;
using MSP.Domain.Base;

namespace MSP.Domain.Entities
{
    public class UserDevice : BaseEntity<Guid>
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FCMToken { get; set; }

        [Required]
        [MaxLength(20)]
        public string Platform { get; set; } // "Android" or "iOS"

        [MaxLength(255)]
        public string? DeviceId { get; set; }

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; }
    }
}
