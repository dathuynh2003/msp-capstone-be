using MSP.Domain.Base;

namespace MSP.Domain.Entities
{
    public class TaskHistory : BaseEntity<Guid>
    {
        public Guid TaskId { get; set; }
        public Guid? FromUserId { get; set; }   // có thể null nếu là người tạo ban đầu
        public Guid? ToUserId { get; set; }

        public string Action { get; set; } // TaskHistoryActionEnum dưới dạng string
        public Guid ChangedById { get; set; }

        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public virtual ProjectTask Task { get; set; }
        public virtual User? FromUser { get; set; }
        public virtual User? ToUser { get; set; }
        public virtual User ChangedBy { get; set; }

    }
}
