using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSP.Shared.Enums;

namespace MSP.Application.Models.Responses.OrganizationInvitation
{
    public class OrganizationInvitationResponse
    {
        public Guid Id { get; set; }
        public Guid BusinessOwnerId { get; set; }
        public string? BusinessOwnerName { get; set; } = string.Empty;
        public string? BusinessOwnerEmail { get; set; }
        public string? BusinessOwnerAvatar { get; set; }
        public string? OrganizationName { get; set; }

        public Guid MemberId { get; set; }
        public string? MemberName { get; set; } = string.Empty;
        public string? MemberEmail { get; set; }
        public string? MemberAvatar { get; set; }

        public InvitationType Type { get; set; }
        public string TypeDisplay => Type == InvitationType.Invite ? "Invite" : "Request";

        public InvitationStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            InvitationStatus.Pending => "Pending",
            InvitationStatus.Accepted => "Accepted",
            InvitationStatus.Rejected => "Rejected",
            InvitationStatus.Canceled => "Canceled",
            _ => "Không xác định"
        };

        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
