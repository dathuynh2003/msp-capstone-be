using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSP.Application.Models.Responses.OrganizationInvitation;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.OrganizationInvitation
{
    public interface IOrganizationInvitationService
    {
        //Business Owner sends invitation to a member
        Task<ApiResponse<bool>> SendInvitationAsync(Guid businessOwnerId, string memberEmail);
        Task<ApiResponse<List<SendInvitationResult>>> SendInvitationListAsync(Guid businessOwnerId, List<string> memberEmails);
        //Member requests to join an organization
        Task<ApiResponse<bool>> RequestJoinOrganizeAsync(Guid memberId, Guid businessOwnerId);
        // BO xem invitations đã gửi cho members
        Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetSentInvitationsByBusinessOwnerIdAsync(Guid businessOwnerId);
        // BO xem requests cần duyệt từ members
        Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetPendingRequestsByBusinessOwnerIdAsync(Guid businessOwnerId);

        // Member xem invitations đã nhận từ BO
        Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetReceivedInvitationsByMemberIdAsync(Guid memberId);

        // Member xem requests đã gửi đến BO
        Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetSentRequestsByMemberIdAsync(Guid memberId);

        // Member accept invitation từ BO
        Task<ApiResponse<string>> MemberAcceptInvitationAsync(Guid memberId, Guid invitationId);

        // BO accept join request từ Member
        Task<ApiResponse<string>> BusinessOwnerAcceptRequestAsync(Guid businessOwnerId, Guid invitationId);

        // Member rời khỏi organization
        Task<ApiResponse<string>> MemberLeaveOrganizationAsync(Guid memberId);

        // Member reject Invitation từ BO
        Task<ApiResponse<string>> MemberRejectInvitationAsync(Guid memberId, Guid invitationId);

        // BO reject join request từ Member
        Task<ApiResponse<string>> BusinessOwnerRejectRequestAsync(Guid businessOwnerId, Guid invitationId);
        Task<ApiResponse<string>> ProcessInvitationAcceptanceAsync(User user, string token);

    }
}
