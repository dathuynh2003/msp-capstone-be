using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.OrganizationInvitation
{
    public class SendInvitationResult
    {
        public string Email { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
