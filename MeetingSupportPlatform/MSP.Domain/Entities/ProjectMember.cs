using MSP.Domain.Base;

namespace MSP.Domain.Entities
{
    public class ProjectMember : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public Guid MemberId { get; set; }
        public virtual User Member { get; set; }

        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
    }
}
