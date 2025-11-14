using MSP.Domain.Base;

namespace MSP.Domain.Entities
{
    public class ProjectTask : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        public Guid? TodoId { get; set; }
        public virtual Todo? Todo { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
        public virtual ICollection<TaskHistory> TaskHistories { get; set; }

        // Navigation: Những todo mới reference task này (Many-to-Many)
        public virtual ICollection<Todo> ReferencingTodos { get; set; } = new List<Todo>();
    }
}
