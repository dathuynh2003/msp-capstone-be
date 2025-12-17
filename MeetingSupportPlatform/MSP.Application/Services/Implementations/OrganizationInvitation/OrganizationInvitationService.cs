using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
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
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;


        public OrganizationInvitationService(
            IOrganizationInviteRepository organizationInviteRepository,
            UserManager<User> userManager,
            IProjectMemberRepository projectMemberRepository,
            IProjectTaskRepository projectTaskRepository,
            IProjectRepository projectRepository,
            INotificationService notificationService,
            IConfiguration configuration)
        {
            _organizationInviteRepository = organizationInviteRepository;
            _userManager = userManager;
            _projectMemberRepository = projectMemberRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _notificationService = notificationService;
            _configuration = configuration;
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
                    MemberId = x.MemberId.Value,
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
                var member = await _userManager.FindByIdAsync(memberId.ToString());
                if (member == null)
                    return ApiResponse<IEnumerable<OrganizationInvitationResponse>>.ErrorResponse(null, "Invalid Member! Cannot fetch invitaions");
                var invitations = await _organizationInviteRepository.GetReceivedInvitationsByMemberIdAsync(memberId);

                var response = invitations.Select(x => new OrganizationInvitationResponse
                {
                    Id = x.Id,
                    BusinessOwnerId = x.BusinessOwnerId,
                    BusinessOwnerName = x.BusinessOwner?.FullName,
                    BusinessOwnerEmail = x.BusinessOwner?.Email,
                    BusinessOwnerAvatar = x.BusinessOwner?.AvatarUrl,
                    OrganizationName = x.BusinessOwner?.Organization,
                    MemberId = x.MemberId.Value,
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
                    MemberId = x.MemberId ?? Guid.Empty,
                    MemberName = x.Member?.FullName ?? x.InvitedEmail,
                    MemberEmail = x.Member?.Email ?? x.InvitedEmail,
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
                    MemberId = x.MemberId.Value,
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

                // 5.1. CHECK & RE-ACTIVATE FORMER PROJECT MEMBERSHIPS
                // Nếu member đã từng rời organization, re-activate các project memberships cũ
                bool isRejoining = false;
                int reactivatedProjectsCount = 0;
                try
                {
                    // Lấy tất cả projects thuộc business owner này
                    var boProjects = await _projectRepository.GetProjectsByBOIdAsync(businessOwner.Id);
                    var projectIds = boProjects.Select(p => p.Id).ToList();

                    if (projectIds.Any())
                    {
                        // Lấy các ProjectMember records cũ (có LeftAt)
                        var allMemberships = await _projectMemberRepository.GetProjectMembersByProjectIdAsync(projectIds.First());
                        var formerMemberships = new List<ProjectMember>();

                        // Get all former memberships across all projects
                        foreach (var projectId in projectIds)
                        {
                            var memberships = await _projectMemberRepository.GetProjectMembersByProjectIdAsync(projectId);
                            formerMemberships.AddRange(
                                memberships.Where(pm => pm.MemberId == memberId && pm.LeftAt.HasValue)
                            );
                        }

                        if (formerMemberships.Any())
                        {
                            isRejoining = true;
                            var now = DateTime.UtcNow;

                            foreach (var membership in formerMemberships)
                            {
                                // Re-activate by clearing LeftAt
                                membership.LeftAt = null;
                                membership.JoinedAt = now; // Update rejoin timestamp
                                await _projectMemberRepository.UpdateAsync(membership);
                                reactivatedProjectsCount++;
                            }

                            await _projectMemberRepository.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the join operation
                    Console.WriteLine($"Failed to re-activate project memberships for member {memberId}: {ex.Message}");
                }

                // 6. Cập nhật invitation hiện tại thành Accepted
                invitation.Status = InvitationStatus.Accepted;
                invitation.RespondedAt = DateTime.UtcNow;
                await _organizationInviteRepository.UpdateAsync(invitation);

                // 7. Cancel tất cả invitations/requests pending khác của member
                var pendingInvitations = await _organizationInviteRepository.GetAllPendingInvitationsByMemberIdAsync(memberId);
                var otherPendingInvitations = pendingInvitations.Where(x => x.Id != invitationId).ToList();
                var pendingExternalInvitations = await _organizationInviteRepository.GetPendingExternalInvitationsByEmailAsync(member.Email!);
                var otherExternalInvitations = pendingExternalInvitations
                    .Where(x => x.Id != invitationId)
                    .ToList();

                var allOtherInvitations = otherPendingInvitations
                    .Union(otherExternalInvitations)
                    .DistinctBy(x => x.Id)
                    .ToList();
                if (allOtherInvitations.Any())
                {
                    foreach (var inv in allOtherInvitations)
                    {
                        inv.Status = InvitationStatus.Canceled;
                        inv.RespondedAt = DateTime.UtcNow;
                    }
                    await _organizationInviteRepository.UpdateRangeAsync(otherPendingInvitations);
                }

                // 8. Send notification to business owner
                try
                {
                    var notificationTitle = isRejoining ? "Member Rejoined" : "Invitation accepted";
                    var notificationMessage = isRejoining
                        ? $"{member.FullName} has rejoined {businessOwner.Organization} and been re-added to {reactivatedProjectsCount} project(s)."
                        : $"{member.FullName} has accepted the invitation and joined {businessOwner.Organization}.";

                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = businessOwner.Id,
                        ActorId = member.Id,
                        Title = notificationTitle,
                        Message = notificationMessage,
                        Type = NotificationTypeEnum.InApp.ToString(),
                        EntityId = invitation.Id.ToString(),
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            InvitationId = invitation.Id,
                            MemberName = member.FullName,
                            MemberEmail = member.Email,
                            MemberId = member.Id,
                            OrganizationName = businessOwner.Organization,
                            EventType = isRejoining ? "MemberRejoined" : "InvitationAccepted",
                            IsRejoining = isRejoining,
                            ReactivatedProjectsCount = reactivatedProjectsCount
                        })
                    };

                    await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                    // Send email notification
                    var emailSubject = isRejoining ? "Member Rejoined" : "Invitation accepted";
                    var emailBody = isRejoining
                        ? $"Hello {businessOwner.FullName},<br/><br/>" +
                          $"<strong>{member.FullName}</strong> has rejoined <strong>{businessOwner.Organization}</strong> and has been automatically re-added to <strong>{reactivatedProjectsCount}</strong> project(s).<br/><br/>" +
                          $"You can continue collaborating on existing projects."
                        : $"Hello {businessOwner.FullName},<br/><br/>" +
                          $"<strong>{member.FullName}</strong> has accepted the invitation and is now a member of <strong>{businessOwner.Organization}</strong>.<br/><br/>" +
                          $"You can add them to projects and start collaborating.";

                    _notificationService.SendEmailNotification(
                        businessOwner.Email!,
                        emailSubject,
                        emailBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }

                var successMessage = isRejoining
                    ? $"Welcome back to {businessOwner.Organization}! You have been re-added to {reactivatedProjectsCount} project(s). {otherPendingInvitations.Count} other pending invitation(s) have been canceled."
                    : $"Successfully joined {businessOwner.Organization}. {otherPendingInvitations.Count} other pending invitation(s) have been canceled.";

                return ApiResponse<string>.SuccessResponse(
                    successMessage,
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

                // Cập nhật ProjectMember - set LeftAt
                var activeMemberships = await _projectMemberRepository.GetActiveMembershipsByMemberIdAsync(memberId);
                var now = DateTime.UtcNow;
                foreach (var pm in activeMemberships)
                {
                    pm.LeftAt = now;
                }
                await _projectMemberRepository.UpdateRangeAsync(activeMemberships);
                await _projectMemberRepository.SaveChangesAsync();

                // XỬ LÝ TASKS CHƯA HOÀN THÀNH: Unassign các task đang được giao cho member
                try
                {
                    var projectIds = activeMemberships.Select(pm => pm.ProjectId).ToList();
                    
                    if (projectIds.Any())
                    {
                        // Lấy tất cả tasks của member trong các projects này
                        var memberTasks = new List<Domain.Entities.ProjectTask>();
                        foreach (var projectId in projectIds)
                        {
                            var tasks = await _projectTaskRepository.GetTasksByProjectIdAsync(projectId);
                            memberTasks.AddRange(tasks.Where(t => t.UserId == memberId));
                        }

                        // Filter tasks chưa hoàn thành (không phải Done hoặc Cancelled)
                        var incompleteTasks = memberTasks
                            .Where(t => t.Status != TaskEnum.Done.ToString() && 
                                       t.Status != TaskEnum.Cancelled.ToString())
                            .ToList();

                        if (incompleteTasks.Any())
                        {
                            foreach (var task in incompleteTasks)
                            {
                                // Unassign task (set UserId = null)
                                task.UserId = null;
                                task.UpdatedAt = now;
                                await _projectTaskRepository.UpdateAsync(task);
                            }

                            await _projectTaskRepository.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the leave operation
                    Console.WriteLine($"Failed to unassign tasks for member {memberId}: {ex.Message}");
                }

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

        public async Task<ApiResponse<string>> ProcessInvitationAcceptanceAsync(User user, string token)
        {
            try
            {
                // 1. Tìm invitation bằng token
                var invitation = await _organizationInviteRepository.GetByTokenAsync(token);

                if (invitation == null)
                {
                    return ApiResponse<string>.ErrorResponse(null, "Invitation not found or invalid.");
                }

                // 2. Validate invitation status
                if (invitation.Status != InvitationStatus.Pending)
                {
                    return ApiResponse<string>.ErrorResponse(null, $"This invitation is already {invitation.Status}.");
                }

                // 3. Validate email match
                if (!string.Equals(invitation.InvitedEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<string>.ErrorResponse(null, "This invitation was sent to a different email address.");
                }

                // 4. Check if user already in an organization
                if (!string.IsNullOrEmpty(user.Organization))
                {
                    return ApiResponse<string>.ErrorResponse(null, "You are already part of an organization.");
                }

                // 5. Get business owner
                var businessOwner = await _userManager.FindByIdAsync(invitation.BusinessOwnerId.ToString());
                if (businessOwner == null)
                {
                    return ApiResponse<string>.ErrorResponse(null, "Organization owner not found.");
                }

                // 6. Update MemberId trước (vì external invitation có MemberId = null)
                invitation.MemberId = user.Id;
                await _organizationInviteRepository.UpdateAsync(invitation);

                // 7. Gọi MemberAcceptInvitationAsync để xử lý phần còn lại
                return await MemberAcceptInvitationAsync(user.Id, invitation.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing invitation: {ex.Message}");
                return ApiResponse<string>.ErrorResponse(null, "Failed to process invitation.");
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

            // Check for existing pending invitation
            //var existingInvitation = await _organizationInviteRepository.IsInvitationExistsAsync(businessOwnerId, member.Id);
            var existingInvitation = member != null
                ? await _organizationInviteRepository.IsInvitationExistsAsync(businessOwnerId, member.Id)
                : await _organizationInviteRepository.IsExternalInvitationExistsAsync(businessOwnerId, memberEmail);
            if (existingInvitation)
                return ApiResponse<bool>.ErrorResponse(false, "You already have a pending invitation for this user.");

            // === MEMBER EXISTS - Internal flow ===
            if (member != null)
            {
                var memberRoles = await _userManager.GetRolesAsync(member);
                if (!memberRoles.Contains(UserRoleEnum.Member.ToString()))
                    return ApiResponse<bool>.ErrorResponse(false, "The specified user is not a member.");

                if (!string.IsNullOrEmpty(member.Organization))
                {
                    return member.ManagedById == businessOwnerId
                        ? ApiResponse<bool>.ErrorResponse(false, "This user is already part of your organization.")
                        : ApiResponse<bool>.ErrorResponse(false, "This user is already part of another organization.");
                }

                // Create internal invitation
                var internalInvitation = new Domain.Entities.OrganizationInvitation
                {
                    BusinessOwnerId = businessOwnerId,
                    MemberId = member.Id,
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await _organizationInviteRepository.AddAsync(internalInvitation);
                if (!result)
                    return ApiResponse<bool>.ErrorResponse(false, "Failed to send invitation.");
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
                        EntityId = internalInvitation.Id.ToString(),
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            InvitationId = internalInvitation.Id,
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

            // === MEMBER NOT EXISTS - External flow ===
            var token = GenerateSecureToken();
            var externalInvitation = new Domain.Entities.OrganizationInvitation
            {
                BusinessOwnerId = businessOwnerId,
                MemberId = null,
                InvitedEmail = memberEmail.ToLower(),
                Token = token,
                //ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                Type = InvitationType.Invite,
                Status = InvitationStatus.Pending
            };
            var addResult = await _organizationInviteRepository.AddAsync(externalInvitation);
            if (!addResult)
                return ApiResponse<bool>.ErrorResponse(false, "Failed to send invitation.");

            // Send email với invitation link
            await SendExternalInvitationEmailAsync(businessOwner, memberEmail, token);

            return ApiResponse<bool>.SuccessResponse(true,
                "Invitation email sent. The user will receive instructions to join.");
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

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private async Task SendExternalInvitationEmailAsync(User businessOwner, string email, string token)
        {
            var clientUrl = _configuration["AppSettings:ClientUrl"];

            // Link đăng ký với email đã điền sẵn
            var signUpLink = $"{clientUrl}/sign-up?email={email}&invitation={token}";

            var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='text-align: center; margin-bottom: 30px;'>
                <h2 style='color: #FF5E13; margin: 0;'>You've Been Invited! 🎉</h2>
            </div>

            <p style='font-size: 16px; color: #333;'>Hello,</p>

            <p style='font-size: 16px; color: #333;'>
                <strong>{businessOwner.FullName}</strong> has invited you to join the organization 
                <strong style='color: #FF5E13;'>{businessOwner.Organization}</strong>.
            </p>

            <div style='background: #FDF0D2; border-radius: 12px; padding: 24px; margin: 24px 0;'>
                <h3 style='color: #FF5E13; margin-top: 0;'>How to join</h3>

                <p style='color: #666; margin: 0 0 16px 0;'>
                    Click the button below to go to the registration page. Your email will be pre-filled automatically.
                </p>

                <p style='text-align: center; margin: 16px 0 24px 0;'>
                    <a href='{signUpLink}' 
                       style='background-color: #FF5E13; color: white; padding: 12px 24px; 
                              text-decoration: none; border-radius: 8px; display: inline-block;
                              font-weight: bold; font-size: 14px;'>
                        Create Account
                    </a>
                </p>

                <p style='color: #666; margin: 0 0 8px 0;'>
                    After that, just complete the registration form and confirm your email.
                </p>
                <p style='color: #666; margin: 0;'>
                    Once your email is confirmed, your invitation will be activated automatically.
                </p>
            </div>

            <div style='background: #f5f5f5; border-radius: 8px; padding: 16px; margin-top: 24px;'>
                <p style='color: #666; font-size: 14px; margin: 0;'>
                    <strong>Your invited email:</strong> {email}
                </p>
            </div>

            <p style='color: #999; font-size: 12px; margin-top: 24px;'>
                If you didn't expect this invitation, you can safely ignore this email.
            </p>
        </div>";

            _notificationService.SendEmailNotification(
                email,
                $"🎉 Invitation to join {businessOwner.Organization}",
                body
            );
        }

    }
}
