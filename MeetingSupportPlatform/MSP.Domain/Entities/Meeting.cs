using MSP.Domain.Base;

namespace MSP.Domain.Entities
{
    public class Meeting : BaseEntity<Guid>
    {
        public Guid CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public Guid? MilestoneId { get; set; }
        public virtual Milestone Milestone { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public string? RecordUrl { get; set; }
        public string? Transcription { get; set; }
        public string? Summary { get; set; }

        public virtual ICollection<User> Attendees { get; set; } = new List<User>();
        public virtual ICollection<Todo> Todos { get; set; } = new List<Todo>();
    }
}
