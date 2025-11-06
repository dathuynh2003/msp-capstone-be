using MSP.Domain.Base;
using MSP.Shared.Enums;

namespace MSP.Domain.Entities
{
    public class Todo : BaseEntity<Guid>
    {
        public Guid MeetingId { get; set; }
        public virtual Meeting Meeting { get; set; }

        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TodoStatus Status { get; set; } = TodoStatus.Generated;

        public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();

        // Reference: Todo liên quan đến task cũ (Many-to-Many)
        public virtual ICollection<ProjectTask> ReferencedTasks { get; set; } = new List<ProjectTask>();

    }
}
