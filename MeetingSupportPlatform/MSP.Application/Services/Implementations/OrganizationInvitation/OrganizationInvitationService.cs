using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Responses.OrganizationInvitation;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.OrganizationInvitation;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.OrganizationInvitation
{
    public class OrganizationInvitationService : IOrganizationInvitationService
    {
        private readonly IOrganizationInviteRepository _organizationInviteRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public OrganizationInvitationService(
            IOrganizationInviteRepository organizationInviteRepository,
            UserManager<User> userManager,
            IProjectMemberRepository projectMemberRepository,
            INotificationService notificationService)
        {
            _organizationInviteRepository = organizationInviteRepository;
            _userManager = userManager;
            _projectMemberRepository = projectMemberRepository;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<string>> BusinessOwnerAcceptRequestAsync(Guid businessOwnerId, Guid invitationId)
        {
            try
            {
                // 1. Lấy request
                var request = await _organizationInviteRepository.GetByIdAsync(invitationId);
                if (request == null)
                {
                    return ApiResponse<string>.ErrorResponse("Request not found.");
                }

                // 2. Validate: Đúng BO không?
                if (request.BusinessOwnerId != businessOwnerId)
                {
                    return ApiResponse<string>.ErrorResponse("You are not authorized to accept this request.");
                }

                // 3. Validate: Phải là Request và Pending
                if (request.Type != InvitationType.Request)
                {
                    return ApiResponse<string>.ErrorResponse("This is not a join request.");
                }
                if (request.Status != InvitationStatus.Pending)
                {
                    return ApiResponse<string>.ErrorResponse($"This request is already {request.Status}.");
                }

                // 4. Lấy member và business owner
                var member = request.Member;
                var businessOwner = await _userManager.FindByIdAsync(businessOwnerId.ToString());
                if (member == null || businessOwner == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found.");
                }

                // 5. Cập nhật Member: Organization và ManagedBy
                member.Organization = businessOwner.Organization;
                member.ManagedById = businessOwner.Id;
                var updateResult = await _userManager.UpdateAsync(member);
                if (!updateResult.Succeeded)
                {
                    return ApiResponse<string>.ErrorResponse("Failed to update member information.");
                }

                // 6. Cập nhật request hiện tại thành Accepted
                request.Status = InvitationStatus.Accepted;
                request.RespondedAt = DateTime.UtcNow;
                await _organizationInviteRepository.UpdateAsync(request);

                // 7. Cancel tất cả invitations/requests pending khác của member
                var pendingInvitations = await _organizationInviteRepository.GetAllPendingInvitationsByMemberIdAsync(member.Id);
                var otherPendingInvitations = pendingInvitations.Where(x => x.Id != invitationId).ToList();

                if (otherPendingInvitations.Any())
                {
                    foreach (var inv in otherPendingInvitations)
                    {
                        inv.Status = InvitationStatus.Canceled;
                        inv.RespondedAt = DateTime.UtcNow;
                    }
                    await _organizationInviteRepository.UpdateRangeAsync(otherPendingInvitations);
                }

                // 8. Send notification to member
                try
                {
                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = member.Id,
                        ActorId = businessOwner.Id,
                        Title = "Join request accepted",
                        Message = $"{businessOwner.FullName} has accepted your join request at {businessOwner.Organization}. Welcome!",
                        Type = NotificationTypeEnum.InApp.ToString(),
                        EntityId = request.Id.ToString(),
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            InvitationId = request.Id,
                            OrganizationName = businessOwner.Organization,
                            BusinessOwnerName = businessOwner.FullName,
                            BusinessOwnerId = businessOwner.Id,
                            EventType = "RequestAccepted"
                        })
                    };

                    await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                    // Send email notification
                    _notificationService.SendEmailNotification(
                        member.Email!,
                        "Join request accepted",
                        $"Hello {member.FullName},<br/><br/>" +
                        $"Great news! {businessOwner.FullName} has accepted your join request to <strong>{businessOwner.Organization}</strong>.<br/><br/>" +
                        $"You are now a member of the organization and can start collaborating.<br/><br/>" +
                        $"Welcome!");
                }
                catch (Exception ex)
                {
                    // Log but don't fail the operation
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }

                return ApiResponse<string>.SuccessResponse(
                    $"{member.FullName} has been added to your organization. {otherPendingInvitations.Count} other pending invitation(s) have been canceled.",
                    "Request accepted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error accepting request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> BusinessOwnerRejectRequestAsync(Guid businessOwnerId, Guid invitationId)
        {
            try
            {
                // 1. Lấy request
                var request = await _organizationInviteRepository.GetByIdAsync(invitationId);
                if (request == null)
                    return ApiResponse<string>.ErrorResponse("Request not found.");

                // 2. Kiểm tra đúng BO và là Request
                if (request.BusinessOwnerId != businessOwnerId)
                    return ApiResponse<string>.ErrorResponse("You are not authorized to reject this request.");
                if (request.Type != InvitationType.Request)
                    return ApiResponse<string>.ErrorResponse("This is not a join request.");

                if (request.Status != InvitationStatus.Pending)
                    return ApiResponse<string>.ErrorResponse($"This request is already {request.Status}.");

                // 3. Đánh dấu reject
                request.Status = InvitationStatus.Rejected;
                request.RespondedAt = DateTime.UtcNow;
                await _organizationInviteRepository.UpdateAsync(request);

                // 4. Send notification to member
                try
                {
                    var member = request.Member;
                    var businessOwner = request.BusinessOwner;

                    if (member != null && businessOwner != null)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = member.Id,
                            ActorId = businessOwner.Id,
                            Title = "Join request rejected",
                            Message = $"Your join request at {businessOwner.Organization} has been rejected by {businessOwner.FullName}.",
                            Type = NotificationTypeEnum.InApp.ToString(),
                            EntityId = request.Id.ToString(),
                            Data = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                InvitationId = request.Id,
                                OrganizationName = businessOwner.Organization,
                                BusinessOwnerName = businessOwner.FullName,
                                BusinessOwnerId = businessOwner.Id,
                                EventType = "RequestRejected"
                            })
                        };

                        await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                        // Send email notification
                        _notificationService.SendEmailNotification(
                            member.Email!,
                            "Join request rejected",
                            $"Hello {member.FullName},<br/><br/>" +
                            $"Unfortunately, {businessOwner.FullName} has rejected your join request to <strong>{businessOwner.Organization}</strong>.<br/><br/>" +
                            $"You can contact the organization administrator for more information.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }

                return ApiResponse<string>.SuccessResponse(
                    "Join request has been rejected.",
                    "Request rejected successfully."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error rejecting request: {ex.Message}");
            }
        }

        // BO xem requests cần duyệt từ members
        public async Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetPendingRequestsByBusinessOwnerIdAsync(Guid businessOwnerId)
        {
            try
            {
                var requests = await _organizationInviteRepository.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId);

                var response = requests.Select(x => new OrganizationInvitationResponse
                {
                    Id = x.Id,
                    BusinessOwnerId = x.BusinessOwnerId,
                    BusinessOwnerName = x.BusinessOwner?.FullName,
                    BusinessOwnerEmail = x.BusinessOwner?.Email,
                    BusinessOwnerAvatar = x.BusinessOwner?.AvatarUrl,
                    OrganizationName = x.BusinessOwner?.Organization,
                    MemberId = x.MemberId,
                    MemberName = x.Member?.FullName,
                    MemberEmail = x.Member?.Email,
                    MemberAvatar = x.Member?.AvatarUrl,
                    Type = x.Type,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    RespondedAt = x.RespondedAt
                }).ToList();

                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.SuccessResponse(
                    response,
                    $"Found {response.Count} pending request(s).");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.ErrorResponse(null, $"Error retrieving pending requests: {ex.Message}");
            }
        }

        // Member xem invitations đã nhận từ BO
        public async Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetReceivedInvitationsByMemberIdAsync(Guid memberId)
        {
            try
            {
                var invitations = await _organizationInviteRepository.GetReceivedInvitationsByMemberIdAsync(memberId);

                var response = invitations.Select(x => new OrganizationInvitationResponse
                {
                    Id = x.Id,
                    BusinessOwnerId = x.BusinessOwnerId,
                    BusinessOwnerName = x.BusinessOwner?.FullName,
                    BusinessOwnerEmail = x.BusinessOwner?.Email,
                    BusinessOwnerAvatar = x.BusinessOwner?.AvatarUrl,
                    OrganizationName = x.BusinessOwner?.Organization,
                    MemberId = x.MemberId,
                    MemberName = x.Member?.FullName,
                    MemberEmail = x.Member?.Email,
                    MemberAvatar = x.Member?.AvatarUrl,
                    Type = x.Type,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    RespondedAt = x.RespondedAt
                }).ToList();

                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.SuccessResponse(
                    response,
                    $"Found {response.Count} received invitation(s).");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.ErrorResponse(null, $"Error retrieving received invitations: {ex.Message}");
            }
        }

        // BO xem invitations đã gửi cho members
        public async Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetSentInvitationsByBusinessOwnerIdAsync(Guid businessOwnerId)
        {
            try
            {
                var invitations = await _organizationInviteRepository.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);

                var response = invitations.Select(x => new OrganizationInvitationResponse
                {
                    Id = x.Id,
                    BusinessOwnerId = x.BusinessOwnerId,
                    BusinessOwnerName = x.BusinessOwner?.FullName,
                    BusinessOwnerEmail = x.BusinessOwner?.Email,
                    BusinessOwnerAvatar = x.BusinessOwner?.AvatarUrl,
                    OrganizationName = x.BusinessOwner?.Organization,
                    MemberId = x.MemberId,
                    MemberName = x.Member?.FullName,
                    MemberEmail = x.Member?.Email,
                    MemberAvatar = x.Member?.AvatarUrl,
                    Type = x.Type,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    RespondedAt = x.RespondedAt
                }).ToList();

                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.SuccessResponse(
                    response,
                    $"Found {response.Count} sent invitation(s).");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.ErrorResponse(null, $"Error retrieving sent invitations: {ex.Message}");
            }
        }

        // Member xem requests đã gửi đến BO
        public async Task<ApiResponse<IEnumerable<OrganizationInvitationResponse>>> GetSentRequestsByMemberIdAsync(Guid memberId)
        {
            try
            {
                var requests = await _organizationInviteRepository.GetSentRequestsByMemberIdAsync(memberId);

                var response = requests.Select(x => new OrganizationInvitationResponse
                {
                    Id = x.Id,
                    BusinessOwnerId = x.BusinessOwnerId,
                    BusinessOwnerName = x.BusinessOwner?.FullName,
                    BusinessOwnerEmail = x.BusinessOwner?.Email,
                    BusinessOwnerAvatar = x.BusinessOwner?.AvatarUrl,
                    OrganizationName = x.BusinessOwner?.Organization,
                    MemberId = x.MemberId,
                    MemberName = x.Member?.FullName,
                    MemberEmail = x.Member?.Email,
                    MemberAvatar = x.Member?.AvatarUrl,
                    Type = x.Type,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    RespondedAt = x.RespondedAt
                }).ToList();

                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.SuccessResponse(
                    response,
                    $"Found {response.Count} sent request(s).");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.ErrorResponse(null, $"Error retrieving sent requests: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> MemberAcceptInvitationAsync(Guid memberId, Guid invitationId)
        {
            try
            {
                // 1. Lấy invitation
                var invitation = await _organizationInviteRepository.GetByIdAsync(invitationId);
                if (invitation == null)
                {
                    return ApiResponse<string>.ErrorResponse("Invitation not found.");
                }

                // 2. Validate: Đúng member không?
                if (invitation.MemberId != memberId)
                {
                    return ApiResponse<string>.ErrorResponse("You are not authorized to accept this invitation.");
                }

                // 3. Validate: Phải là Invite và Pending
                if (invitation.Type != InvitationType.Invite)
                {
                    return ApiResponse<string>.ErrorResponse("This is not an invitation.");
                }
                if (invitation.Status != InvitationStatus.Pending)
                {
                    return ApiResponse<string>.ErrorResponse($"This invitation is already {invitation.Status}.");
                }

                // 4. Lấy member và business owner
                var member = await _userManager.FindByIdAsync(memberId.ToString());
                var businessOwner = invitation.BusinessOwner;

                if (member == null || businessOwner == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found.");
                }

                // 5. Cập nhật Member: Organization và ManagedBy
                member.Organization = businessOwner.Organization;
                member.ManagedById = businessOwner.Id;

                var updateResult = await _userManager.UpdateAsync(member);
                if (!updateResult.Succeeded)
                {
                    return ApiResponse<string>.ErrorResponse("Failed to update member information.");
                }

                // 6. Cập nhật invitation hiện tại thành Accepted
                invitation.Status = InvitationStatus.Accepted;
                invitation.RespondedAt = DateTime.UtcNow;
                await _organizationInviteRepository.UpdateAsync(invitation);

                // 7. Cancel tất cả invitations/requests pending khác của member
                var pendingInvitations = await _organizationInviteRepository.GetAllPendingInvitationsByMemberIdAsync(memberId);
                var otherPendingInvitations = pendingInvitations.Where(x => x.Id != invitationId).ToList();

                if (otherPendingInvitations.Any())
                {
                    foreach (var inv in otherPendingInvitations)
                    {
                        inv.Status = InvitationStatus.Canceled;
                        inv.RespondedAt = DateTime.UtcNow;
                    }
                    await _organizationInviteRepository.UpdateRangeAsync(otherPendingInvitations);
                }

                // 8. Send notification to business owner
                try
                {
                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = businessOwner.Id,
                        ActorId = member.Id,
                        Title = "Invitation accepted",
                        Message = $"{member.FullName} has accepted the invitation and joined {businessOwner.Organization}.",
                        Type = NotificationTypeEnum.InApp.ToString(),
                        EntityId = invitation.Id.ToString(),
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            InvitationId = invitation.Id,
                            MemberName = member.FullName,
                            MemberEmail = member.Email,
                            MemberId = member.Id,
                            OrganizationName = businessOwner.Organization,
                            EventType = "InvitationAccepted"
                        })
                    };

                    await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                    // Send email notification
                    _notificationService.SendEmailNotification(
                        businessOwner.Email!,
                        "Invitation accepted",
                        $"Hello {businessOwner.FullName},<br/><br/>" +
                        $"<strong>{member.FullName}</strong> has accepted the invitation and is now a member of <strong>{businessOwner.Organization}</strong>.<br/><br/>" +
                        $"You can add them to projects and start collaborating.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }

                return ApiResponse<string>.SuccessResponse(
                    $"Successfully joined {businessOwner.Organization}. {otherPendingInvitations.Count} other pending invitation(s) have been canceled.",
                    "Invitation accepted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error accepting invitation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> MemberLeaveOrganizationAsync(Guid memberId)
        {
            try
            {
                var member = await _userManager.FindByIdAsync(memberId.ToString());
                if (member == null)
                    return ApiResponse<string>.ErrorResponse("Member not found.");

                if (member.ManagedById == null && string.IsNullOrEmpty(member.Organization))
                    return ApiResponse<string>.ErrorResponse("You are not a member of any organization.");

                var oldOrganization = member.Organization;
                member.Organization = null;
                member.ManagedById = null;

                var updateResult = await _userManager.UpdateAsync(member);
                if (!updateResult.Succeeded)
                    return ApiResponse<string>.ErrorResponse("Failed to leave organization.");

                // Sử dụng repository thay vì context
                var activeMemberships = await _projectMemberRepository.GetActiveMembershipsByMemberIdAsync(memberId);
                var now = DateTime.UtcNow;
                foreach (var pm in activeMemberships)
                {
                    pm.LeftAt = now;
                }
                await _projectMemberRepository.UpdateRangeAsync(activeMemberships);
                await _projectMemberRepository.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(
                    $"You have successfully left {oldOrganization ?? "the organization"}. Project memberships updated ({activeMemberships.Count}).",
                    "Left organization successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error leaving organization: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> MemberRejectInvitationAsync(Guid memberId, Guid invitationId)
        {
            try
            {
                // 1. Lấy invitation
                var invitation = await _organizationInviteRepository.GetByIdAsync(invitationId);
                if (invitation == null)
                    return ApiResponse<string>.ErrorResponse("Invitation not found.");

                // 2. Kiểm tra đúng member và là loại Invite đang Pending
                if (invitation.MemberId != memberId)
                    return ApiResponse<string>.ErrorResponse("You are not authorized to reject this invitation.");
                if (invitation.Type != InvitationType.Invite)
                    return ApiResponse<string>.ErrorResponse("This is not a business invitation.");
                if (invitation.Status != InvitationStatus.Pending)
                    return ApiResponse<string>.ErrorResponse($"This invitation is already {invitation.Status}.");

                // 3. Cập nhật status thành Rejected
                invitation.Status = InvitationStatus.Rejected;
                invitation.RespondedAt = DateTime.UtcNow;
                await _organizationInviteRepository.UpdateAsync(invitation);

                // 4. Send notification to business owner
                try
                {
                    var member = invitation.Member;
                    var businessOwner = invitation.BusinessOwner;

                    if (member != null && businessOwner != null)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = businessOwner.Id,
                            ActorId = member.Id,
                            Title = "Invitation rejected",
                            Message = $"{member.FullName} has rejected the invitation to join {businessOwner.Organization}.",
                            Type = NotificationTypeEnum.InApp.ToString(),
                            EntityId = invitation.Id.ToString(),
                            Data = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                InvitationId = invitation.Id,
                                MemberName = member.FullName,
                                MemberEmail = member.Email,
                                MemberId = member.Id,
                                OrganizationName = businessOwner.Organization,
                                EventType = "InvitationRejected"
                            })
                        };

                        await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                        // Send email notification
                        _notificationService.SendEmailNotification(
                            businessOwner.Email!,
                            "Invitation rejected",
                            $"Hello {businessOwner.FullName},<br/><br/>" +
                            $"<strong>{member.FullName}</strong> has rejected the invitation to join <strong>{businessOwner.Organization}</strong>.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }

                return ApiResponse<string>.SuccessResponse(
                    $"You have rejected the invitation from business.",
                    "Invitation rejected successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error rejecting invitation: {ex.Message}");
            }
        }


        public async Task<ApiResponse<bool>> RequestJoinOrganizeAsync(Guid memberId, Guid businessOwnerId)
        {
            // Validate member exists
            var member = await _userManager.FindByIdAsync(memberId.ToString());
            if (member == null)
            {
                return ApiResponse<bool>.ErrorResponse(false, "Member not found.");
            }

            // Check if member already has organization
            if (!string.IsNullOrEmpty(member.Organization))
            {
                return ApiResponse<bool>.ErrorResponse(false,
                    "You are already part of an organization. Please leave your current organization first.");
            }

            // Validate business owner exists
            var businessOwner = await _userManager.FindByIdAsync(businessOwnerId.ToString());
            if (businessOwner == null)
            {
                return ApiResponse<bool>.ErrorResponse(false, "Business owner not found.");
            }
            
            //Check businessOwner role
            var roles = await _userManager.GetRolesAsync(businessOwner);
            if (!roles.Contains("BusinessOwner"))
            {
                return ApiResponse<bool>.ErrorResponse(false, "The specified user is not a business owner.");
            }

            // Check if invitation already exists
            var invitationExists = await _organizationInviteRepository.IsInvitationExistsAsync(businessOwnerId, memberId);
            if (invitationExists)
            {
                return ApiResponse<bool>.ErrorResponse(false, "An invitation already exists.");
            }
            
            // Create new invitation
            var invitation = new Domain.Entities.OrganizationInvitation
            {
                BusinessOwnerId = businessOwnerId,
                MemberId = memberId,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Type = InvitationType.Request
            };

            var rs = await _organizationInviteRepository.AddAsync(invitation);
            if (!rs)
            {
                return ApiResponse<bool>.ErrorResponse(false, "Failed to send request to join organization.");
            }

            // Send notification to business owner
            try
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = businessOwner.Id,
                    ActorId = member.Id,
                    Title = "New join request",
                    Message = $"{member.FullName} has sent a join request to your organization {businessOwner.Organization}.",
                    Type = NotificationTypeEnum.InApp.ToString(),
                    EntityId = invitation.Id.ToString(),
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        InvitationId = invitation.Id,
                        MemberName = member.FullName,
                        MemberEmail = member.Email,
                        MemberId = member.Id,
                        OrganizationName = businessOwner.Organization,
                        EventType = "JoinRequest"
                    })
                };

                await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                // Send email notification
                _notificationService.SendEmailNotification(
                    businessOwner.Email!,
                    "New join request",
                    $"Hello {businessOwner.FullName},<br/><br/>" +
                    $"<strong>{member.FullName}</strong> ({member.Email}) has sent a join request to your organization <strong>{businessOwner.Organization}</strong>.<br/><br/>" +
                    $"Please review and respond to this request on your dashboard.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Request to join organization sent successfully.");
        }

        public async Task<ApiResponse<bool>> SendInvitationAsync(Guid businessOwnerId, string memberEmail)
        {
            // Validate business owner exists
            var businessOwner = await _userManager.FindByIdAsync(businessOwnerId.ToString());
            if (businessOwner == null)
            {
                return ApiResponse<bool>.ErrorResponse(false, "Business owner not found.");
            }
            
            // Verify businessOwner has BusinessOwner role
            var roles = await _userManager.GetRolesAsync(businessOwner);
            if (!roles.Contains(UserRoleEnum.BusinessOwner.ToString()))
            {
                return ApiResponse<bool>.ErrorResponse(false,
                    "You must be a business owner to send invitations.");
            }
            
            // Validate business owner has organization
            if (string.IsNullOrEmpty(businessOwner.Organization))
            {
                return ApiResponse<bool>.ErrorResponse(false,
                    "You must have an organization to send invitations.");
            }

            // Find member by email
            var member = await _userManager.FindByEmailAsync(memberEmail.ToUpper());
            if (member == null)
            {
                return ApiResponse<bool>.ErrorResponse(false,
                    $"No user found with email: {memberEmail}");
            }
            
            // Verify member has Member role
            var memberRoles = await _userManager.GetRolesAsync(member);
            if (!memberRoles.Contains(UserRoleEnum.Member.ToString()))
            {
                return ApiResponse<bool>.ErrorResponse(false,
                    "The specified user is not a member.");
            }

            // Check if member already has organization
            if (!string.IsNullOrEmpty(member.Organization))
            {
                if (member.ManagedById == businessOwnerId)
                {
                    return ApiResponse<bool>.ErrorResponse(false,
                        "This user is already part of your organization.");
                }
                else
                {
                    return ApiResponse<bool>.ErrorResponse(false,
                        "This user is already part of another organization.");
                }
            }

            // Check for existing pending invitation
            var existingInvitation = await _organizationInviteRepository.IsInvitationExistsAsync(businessOwnerId, member.Id);

            if (existingInvitation)
            {
                return ApiResponse<bool>.ErrorResponse(false,
                    "You already have a pending invitation for this user.");
            }

            // Create new invitation
            var invitation = new Domain.Entities.OrganizationInvitation
            {
                BusinessOwnerId = businessOwnerId,
                MemberId = member.Id,
                CreatedAt = DateTime.UtcNow,
                Status = InvitationStatus.Pending,
                Type = InvitationType.Invite
            };

            var result = await _organizationInviteRepository.AddAsync(invitation);
            if (!result)
            {
                return ApiResponse<bool>.ErrorResponse(false, "Failed to send invitation.");
            }

            // Send notification to member
            try
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = member.Id,
                    ActorId = businessOwner.Id,
                    Title = "Organization Invitation",
                    Message = $"{businessOwner.FullName} has invited you to join the organization {businessOwner.Organization}.",
                    Type = NotificationTypeEnum.InApp.ToString(),
                    EntityId = invitation.Id.ToString(),
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        InvitationId = invitation.Id,
                        OrganizationName = businessOwner.Organization,
                        BusinessOwnerName = businessOwner.FullName,
                        BusinessOwnerEmail = businessOwner.Email,
                        BusinessOwnerId = businessOwner.Id,
                        EventType = "OrganizationInvitation"
                    })
                };

                await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                // Send email notification
                _notificationService.SendEmailNotification(
                    member.Email!,
                    "Organization Invitation",
                    $"Hello {member.FullName},<br/><br/>" +
                    $"You have been invited to join the organization <strong>{businessOwner.Organization}</strong> by <strong>{businessOwner.FullName}</strong>.<br/><br/>" +
                    $"Please log in to your dashboard to accept or decline the invitation.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Invitation sent successfully.");
        }

        public async Task<ApiResponse<List<SendInvitationResult>>> SendInvitationListAsync(Guid businessOwnerId, List<string> memberEmails)
        {
            var results = new List<SendInvitationResult>();
            foreach (var memberEmail in memberEmails.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var result = await SendInvitationAsync(businessOwnerId, memberEmail);
                results.Add(new SendInvitationResult
                {
                    Email = memberEmail,
                    Success = result.Success && result.Data,
                    Message = result.Message
                });
            }
            return ApiResponse<List<SendInvitationResult>>.SuccessResponse(
                results,
                "Invitations processed."
            );
        }
    }
}
