using System.ComponentModel.DataAnnotations;
using MSP.Shared.Enums;

namespace MSP.Domain.Entities
{
    public class OrganizationInvitation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BusinessOwnerId { get; set; }
        public virtual User BusinessOwner { get; set; } = null!;

        public Guid MemberId { get; set; }
        public virtual User Member { get; set; } = null!;

        public InvitationType Type { get; set; }
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}
