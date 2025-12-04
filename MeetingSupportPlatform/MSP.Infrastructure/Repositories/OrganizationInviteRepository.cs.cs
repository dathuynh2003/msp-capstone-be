using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using MSP.Shared.Enums;

namespace MSP.Infrastructure.Repositories
{
    public class OrganizationInvitationRepository : IOrganizationInviteRepository
    {
        private readonly ApplicationDbContext _context;
        public OrganizationInvitationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(OrganizationInvitation invitation)
        {
            await _context.OrganizationInvitations.AddAsync(invitation);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // BO xem invitations ĐÃ GỬI cho members
        public async Task<IEnumerable<OrganizationInvitation>> GetSentInvitationsByBusinessOwnerIdAsync(Guid businessOwnerId)
        {
            return await _context.OrganizationInvitations
                .Where(x =>
                    x.BusinessOwnerId == businessOwnerId &&
                    x.Type == InvitationType.Invite)
                .Include(x => x.Member)
                .Include(x => x.BusinessOwner)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // BO xem requests CẦN DUYỆT từ members
        public async Task<IEnumerable<OrganizationInvitation>> GetPendingRequestsByBusinessOwnerIdAsync(Guid businessOwnerId)
        {
            return await _context.OrganizationInvitations
                .Where(x =>
                    x.BusinessOwnerId == businessOwnerId &&
                    x.Type == InvitationType.Request &&  // ✅ Member gửi request
                    x.Status == InvitationStatus.Pending)
                .Include(x => x.Member)
                .Include(x => x.BusinessOwner)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // Member xem invitations ĐÃ NHẬN từ BO
        public async Task<IEnumerable<OrganizationInvitation>> GetReceivedInvitationsByMemberIdAsync(Guid memberId)
        {
            return await _context.OrganizationInvitations
                .Where(x =>
                    x.MemberId == memberId &&
                    x.Type == InvitationType.Invite &&
                    x.Status == InvitationStatus.Pending)
                .Include(x => x.BusinessOwner)
                .Include(x => x.Member)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        //Member xem requests ĐÃ GỬI đến BO
        public async Task<IEnumerable<OrganizationInvitation>> GetSentRequestsByMemberIdAsync(Guid memberId)
        {
            return await _context.OrganizationInvitations
                .Where(x =>
                    x.MemberId == memberId &&
                    x.Type == InvitationType.Request)
                .Include(x => x.BusinessOwner)
                .Include(x => x.Member)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsInvitationExistsAsync(Guid businessOwnerId, Guid memberId)
        {
            return await _context.OrganizationInvitations
                .AnyAsync(x =>
                    x.BusinessOwnerId == businessOwnerId &&
                    x.MemberId == memberId &&
                    x.Status == Shared.Enums.InvitationStatus.Pending);
        }

        public async Task<OrganizationInvitation?> GetByIdAsync(Guid id)
        {
            return await _context.OrganizationInvitations
                .Include(x => x.BusinessOwner)
                .Include(x => x.Member)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(OrganizationInvitation invitation)
        {
            _context.OrganizationInvitations.Update(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrganizationInvitation>> GetAllPendingInvitationsByMemberIdAsync(Guid memberId)
        {
            return await _context.OrganizationInvitations
                .Where(x => x.MemberId == memberId && x.Status == InvitationStatus.Pending)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<OrganizationInvitation> invitations)
        {
            _context.OrganizationInvitations.UpdateRange(invitations);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrganizationInvitation>> GetExpiredPendingInvitationsAsync(DateTime expiryDate)
        {
            return await _context.OrganizationInvitations
                .Where(x =>
                    x.Status == InvitationStatus.Pending &&
                    x.CreatedAt < expiryDate)
                .Include(x => x.BusinessOwner)
                .Include(x => x.Member)
                .ToListAsync();
        }

        public Task<int> CountMembersInOrganizationAsync(Guid businessOwnerId)
        {
            return _context.OrganizationInvitations
                .Where(x =>
                    x.BusinessOwnerId == businessOwnerId &&
                    x.Status == InvitationStatus.Accepted
                //&& (!startDate.HasValue || (x.RespondedAt.HasValue && x.RespondedAt.Value.Date >= startDate.Value.Date)) &&
                //(!endDate.HasValue || (x.RespondedAt.HasValue && x.RespondedAt.Value.Date <= endDate.Value.Date))
                )
                .CountAsync();
        }

        public async Task<bool> IsExternalInvitationExistsAsync(Guid businessOwnerId, string email)
        {
            return await _context.OrganizationInvitations
                .AnyAsync(x => x.BusinessOwnerId == businessOwnerId
                    && x.InvitedEmail == email.ToLower()
                    && x.Status == InvitationStatus.Pending);
        }

        public async Task<OrganizationInvitation?> GetByTokenAsync(string token)
        {
            return await _context.OrganizationInvitations
                .Include(x => x.BusinessOwner)
                .FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}
