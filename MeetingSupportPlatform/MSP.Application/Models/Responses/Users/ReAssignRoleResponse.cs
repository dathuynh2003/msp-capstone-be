using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Users
{
    public class ReAssignRoleResponse
    {
        public Guid UserId { get; set; }
        public string NewRole { get; set; }
    }
}
