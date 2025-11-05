using MSP.Domain.Base;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Domain.Entities
{
    public class TaskReassignRequest : BaseEntity<Guid>
    {
        public Guid TaskId { get; set; }
        public Guid FromUserId { get; set; } 
        public Guid ToUserId { get; set; }
        public string Status { get; set; } = TaskReassignEnum.Pending.ToString(); // PENDING / ACCEPTED / REJECTED
        public string Description { get; set; }  
        public DateTime? RespondedAt { get; set; }
        public string? ResponseMessage { get; set; }
        public virtual ProjectTask Task { get; set; }
        public virtual User FromUser { get; set; }
        public virtual User ToUser { get; set; }

    }
}
