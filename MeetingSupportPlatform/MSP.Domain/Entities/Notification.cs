using MSP.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace MSP.Domain.Entities
{
    public class Notification : BaseEntity<Guid>
    {
        [Required]
        public Guid UserId { get; set; }
        public Guid? ActorId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public string? Type { get; set; } // Meeting, Task, Project, InApp, ...
        public string? EntityId { get; set; }
        public bool IsRead { get; set; } = false;
        public string? Data { get; set; } // JSON data for additional information
        public virtual User User { get; set; }
    }
}

