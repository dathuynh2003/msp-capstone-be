using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.User
{
    public class ReAssignRoleRequest
    {
        public Guid BusinessOwnerId { get; set; }
        public Guid UserId { get; set; }
        public string NewRole { get; set; }
    }
}
