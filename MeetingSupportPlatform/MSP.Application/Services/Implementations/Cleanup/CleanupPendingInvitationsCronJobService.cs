using Microsoft.Extensions.Logging;
using MSP.Application.Repositories;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.Cleanup
{
    /// <summary>
    /// Service to automatically cancel/expire pending organization invitations after X days
    /// Helps keep database clean and improves user experience
    /// </summary>
    public class CleanupPendingInvitationsCronJobService
    {
        private readonly IOrganizationInviteRepository _organizationInviteRepository;
        private readonly ILogger<CleanupPendingInvitationsCronJobService> _logger;
        private const int EXPIRY_DAYS = 7; // Invitations expire after 7 days

        public CleanupPendingInvitationsCronJobService(
            IOrganizationInviteRepository organizationInviteRepository,
            ILogger<CleanupPendingInvitationsCronJobService> logger)
        {
            _organizationInviteRepository = organizationInviteRepository;
            _logger = logger;
        }

        /// <summary>
        /// Cleanup expired pending invitations
        /// This method will be called by Hangfire Recurring Job
        /// </summary>
        public async Task CleanupExpiredInvitationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to cleanup expired pending invitations at {Time}", DateTime.UtcNow);

                var now = DateTime.UtcNow;
                var expiryDate = now.AddDays(-EXPIRY_DAYS);

                // Get pending invitations that have exceeded EXPIRY_DAYS
                var expiredInvitations = await _organizationInviteRepository.GetExpiredPendingInvitationsAsync(expiryDate);

                if (expiredInvitations.Any())
                {
                    _logger.LogInformation("Found {Count} expired pending invitations", expiredInvitations.Count());

                    foreach (var invitation in expiredInvitations)
                    {
                        var typeText = invitation.Type == InvitationType.Invite ? "Invitation" : "Request";
                        var fromText = invitation.Type == InvitationType.Invite
                            ? $"from BusinessOwner {invitation.BusinessOwnerId}"
                            : $"from Member {invitation.MemberId}";

                        // Clear token if exists
                        if (!string.IsNullOrEmpty(invitation.Token))
                        {
                            _logger.LogInformation(
                                "Clearing token for expired {Type} {InvitationId}",
                                typeText,
                                invitation.Id);

                            invitation.Token = null;
                        }

                        _logger.LogInformation(
                            "{Type} {InvitationId} {FromText} created at {CreatedAt} has been expired and marked as Canceled",
                            typeText,
                            invitation.Id,
                            fromText,
                            invitation.CreatedAt);

                        // Mark as Canceled (or could create a new Expired status)
                        invitation.Status = InvitationStatus.Canceled;
                        invitation.RespondedAt = now;

                        await _organizationInviteRepository.UpdateAsync(invitation);
                    }

                    _logger.LogInformation("Successfully cleaned {Count} expired invitations", expiredInvitations.Count());
                }
                else
                {
                    _logger.LogInformation("No expired pending invitations found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired invitations");
                throw; // Rethrow for Hangfire to retry if needed
            }
        }
    }
}
