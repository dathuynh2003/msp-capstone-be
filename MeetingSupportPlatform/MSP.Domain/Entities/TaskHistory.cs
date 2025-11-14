using MSP.Domain.Base;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Domain.Entities
{
    public class TaskHistory : BaseEntity<Guid>
    {
        public Guid TaskId { get; set; }
        public Guid? FromUserId { get; set; }   // có thể null nếu là người tạo ban đầu
        public Guid ToUserId { get; set; }
        public virtual ProjectTask Task { get; set; }
        public virtual User? FromUser { get; set; }
        public virtual User ToUser { get; set; }

    }
}
