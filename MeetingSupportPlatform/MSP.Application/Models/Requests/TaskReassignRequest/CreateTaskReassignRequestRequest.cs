using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.TaskReassignRequest
{
    public class CreateTaskReassignRequestRequest
    {
        public Guid TaskId { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public string Description { get; set; }

    }
}
