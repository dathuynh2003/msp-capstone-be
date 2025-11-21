using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSP.Domain.Entities;

namespace MSP.Application.Repositories
{
    public interface IOrganizationInviteRepository
    {
        Task<bool> IsInvitationExistsAsync(Guid businessOwnerId, Guid memberId);
        Task<bool> AddAsync(OrganizationInvitation invitation);
        Task<IEnumerable<OrganizationInvitation>> GetSentInvitationsByBusinessOwnerIdAsync(Guid businessOwnerId);
        Task<IEnumerable<OrganizationInvitation>> GetPendingRequestsByBusinessOwnerIdAsync(Guid businessOwnerId);
        Task<IEnumerable<OrganizationInvitation>> GetReceivedInvitationsByMemberIdAsync(Guid memberId);
        Task<IEnumerable<OrganizationInvitation>> GetSentRequestsByMemberIdAsync(Guid memberId);
        Task<OrganizationInvitation?> GetByIdAsync(Guid id);
        Task UpdateAsync(OrganizationInvitation invitation);
        Task<IEnumerable<OrganizationInvitation>> GetAllPendingInvitationsByMemberIdAsync(Guid memberId);
        Task UpdateRangeAsync(IEnumerable<OrganizationInvitation> invitations);
        /// <summary>
        /// Lấy pending invitations đã quá expiry date
        /// </summary>
        Task<IEnumerable<OrganizationInvitation>> GetExpiredPendingInvitationsAsync(DateTime expiryDate);

        //count number of members in organization in subscription period
        Task<int> CountMembersInOrganizationAsync(Guid businessOwnerId, DateTime? startDate, DateTime? endDate);
    }
}
