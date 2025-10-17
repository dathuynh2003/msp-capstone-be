using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.OrganizationInvitatio
{
    public class SendInvitationListRequest
    {
        public List<string> MemberEmails { get; set; } = new();
    }
}
