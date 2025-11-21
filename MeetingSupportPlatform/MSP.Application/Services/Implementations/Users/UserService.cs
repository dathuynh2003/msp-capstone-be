using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Requests.User;
using MSP.Application.Models.Responses.Users;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Users;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;
using MSP.Shared.Specifications;

namespace MSP.Application.Services.Implementations.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IOrganizationInviteRepository _organizationInviteRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IPackageRepository _packageRepository;
        public UserService(UserManager<User> userManager, IUserRepository userRepository, INotificationService notificationService, IOrganizationInviteRepository organizationInviteRepository, IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository, ISubscriptionRepository subscriptionRepository, IPackageRepository packageRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _organizationInviteRepository = organizationInviteRepository;
            _projectMemberRepository = projectMemberRepository;
            _projectRepository = projectRepository;
            _subscriptionRepository = subscriptionRepository;
            _packageRepository = packageRepository;
        }

        public async Task<ApiResponse<IEnumerable<UserResponse>>> GetBusinessOwnersAsync()
        {
            try
            {
                var businessOwners = await _userRepository.GetBusinessOwnersAsync();
                var results = businessOwners.Select(user => new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    PhoneNumber = user.PhoneNumber,
                    Organization = user.Organization,
                    BusinessLicense = user.BusinessLicense,
                    IsApproved = user.IsApproved,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                });
                return ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(results, "Business owners retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<UserResponse>>.ErrorResponse(null, $"Error retrieving business owners: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<UserResponse>>> GetPendingBusinessOwnersAsync()
        {
            try
            {
                var pendingBusinessOwners = await _userRepository.GetPendingBusinessOwnersAsync();
                var results = pendingBusinessOwners.Select(user => new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Organization = user.Organization,
                    BusinessLicense = user.BusinessLicense,
                    IsApproved = user.IsApproved,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                });
                return ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(results, "Pending business owners retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<UserResponse>>.ErrorResponse(null, $"Error retrieving pending business owners: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ApproveBusinessOwnerAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(UserRoleEnum.BusinessOwner.ToString()))
            {
                return ApiResponse<string>.ErrorResponse(null, "User is not a BusinessOwner.");
            }

            if (user.IsApproved)
            {
                return ApiResponse<string>.SuccessResponse("User is already approved.", "User is already approved.");
            }

            user.IsApproved = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return ApiResponse<string>.ErrorResponse(null, "Failed to approve user.");
            }

            // Gửi notification cho user
            await _notificationService.CreateInAppNotificationAsync(new CreateNotificationRequest
            {
                UserId = user.Id,
                Title = "Account approved",
                Message = $"Welcome {user.FullName}! Your BusinessOwner account has been approved by admin. You can now log in to the system.",
                Type = MSP.Shared.Enums.NotificationTypeEnum.InApp.ToString(),
                Data = $"{{\"eventType\":\"AccountApproved\",\"userId\":\"{user.Id}\"}}"
            });


            // Gửi email thông báo BusinessOwner được duyệt
            var emailBody = EmailNotificationTemplate.BusinessOwnerStatusNotification(user.FullName, true);

            _notificationService.SendEmailNotification(
                user.Email,
                "BusinessOwner Account Approved - Meeting Support Platform",
                emailBody
            );


            // Add free subscription package
            var freePackage = await _packageRepository.GetFreePackageAsync();
            if (freePackage != null)
            {
                var subscription = new Subscription
                {
                    UserId = user.Id,
                    PackageId = freePackage.Id,
                    TotalPrice = 0,
                    TransactionID = "FREE_PACKAGE",
                    PaymentMethod = "System",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Status = PaymentEnum.Paid.ToString().ToUpper(),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(freePackage.BillingCycle),
                };
                await _subscriptionRepository.AddAsync(subscription);
                await _subscriptionRepository.SaveChangesAsync();
            }

            return ApiResponse<string>.SuccessResponse("BusinessOwner approved successfully!", "User has been approved as BusinessOwner successfully.");
        }

        public async Task<ApiResponse<string>> RejectBusinessOwnerAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(UserRoleEnum.BusinessOwner.ToString()))
            {
                return ApiResponse<string>.ErrorResponse(null, "User is not a BusinessOwner.");
            }

            if (user.IsApproved)
            {
                return ApiResponse<string>.ErrorResponse(null, "Cannot reject an already approved user.");
            }

            // Gửi notification cho user
            await _notificationService.CreateInAppNotificationAsync(new CreateNotificationRequest
            {
                UserId = user.Id,
                Title = "Account rejected",
                Message = $"Hello {user.FullName}, your BusinessOwner account has been rejected. Please contact admin for more details.",
                Type = MSP.Shared.Enums.NotificationTypeEnum.InApp.ToString(),
                Data = $"{{\"eventType\":\"AccountRejected\",\"userId\":\"{user.Id}\"}}"
            });


            // Gửi email thông báo BusinessOwner bị từ chối
            var emailBody = EmailNotificationTemplate.BusinessOwnerStatusNotification(user.FullName, false);

            _notificationService.SendEmailNotification(
                user.Email,
                "BusinessOwner Request Not Approved - Meeting Support Platform",
                emailBody
            );

            return ApiResponse<string>.SuccessResponse("BusinessOwner rejected successfully!", "User BusinessOwner request has been rejected successfully.");
        }

        public async Task<ApiResponse<string>> ToggleUserActiveStatusAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "User not found.");
            }

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return ApiResponse<string>.ErrorResponse(null, "Failed to update user status.");
            }

            var status = user.IsActive ? "activated" : "deactivated";
            var statusMessage = user.IsActive ? "User activated successfully." : "User deactivated successfully.";

            return ApiResponse<string>.SuccessResponse($"User {status} successfully!", statusMessage);
        }

        public async Task<ApiResponse<IEnumerable<GetUserResponse>>> GetMembersManagedByAsync(Guid businessOwnerId)
        {
            var businessOwner = await _userManager.FindByIdAsync(businessOwnerId.ToString());
            var members = await _userRepository.GetMembersManagedByAsync(businessOwnerId);

            var memberRoles = new Dictionary<Guid, string>();
            foreach (var user in members)
            {
                var roles = await _userManager.GetRolesAsync(user);
                memberRoles[user.Id] = roles.FirstOrDefault() ?? "Member";
            }

            var results = members.Select(user => new GetUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                PhoneNumber = user.PhoneNumber,
                Organization = user.Organization,
                ManagedBy = businessOwnerId,
                ManagerName = businessOwner?.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                RoleName = memberRoles[user.Id] // Lấy từ dictionary
            });

            return ApiResponse<IEnumerable<GetUserResponse>>.SuccessResponse(
                results,
                "Members retrieved successfully."
            );
        }


        public async Task<ApiResponse<ReAssignRoleResponse>> ReAssignRoleAsync(ReAssignRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return ApiResponse<ReAssignRoleResponse>.ErrorResponse(null, "User not found.");
            }

            if (user.ManagedById == null || user.ManagedById != request.BusinessOwnerId)
            {
                return ApiResponse<ReAssignRoleResponse>.ErrorResponse(null, "You do not have permission to change this user's role.");
            }

            var newRole = request.NewRole;

            // Chỉ cho phép ProjectManager và Member
            var allowedRoles = new[] {
                UserRoleEnum.ProjectManager.ToString(),
                UserRoleEnum.Member.ToString()
            };

            if (!allowedRoles.Contains(newRole))
            {
                return ApiResponse<ReAssignRoleResponse>.ErrorResponse(null, "Invalid role specified. Only ProjectManager and Member roles are allowed.");
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return ApiResponse<ReAssignRoleResponse>.ErrorResponse(null, "Failed to remove user from current roles.");
            }
            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                return ApiResponse<ReAssignRoleResponse>.ErrorResponse(null, "Failed to assign new role to user.");
            }
            var response = new ReAssignRoleResponse
            {
                UserId = user.Id,
                NewRole = newRole
            };
            return ApiResponse<ReAssignRoleResponse>.SuccessResponse(response, "User role reassigned successfully.");
        }

        public async Task<ApiResponse<UserDetailResponse>> GetUserDetailByIdAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.ManagedBy)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ApiResponse<UserDetailResponse>.ErrorResponse(null, "User not found.");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var response = new UserDetailResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Organization = user.Organization,
                ManagedBy = user.ManagedById,
                ManagerName = user.ManagedBy?.FullName,
                CreatedAt = user.CreatedAt,
                RoleName = roles.FirstOrDefault() ?? "Member"
            };
            return ApiResponse<UserDetailResponse>.SuccessResponse(response, "User details retrieved successfully.");
        }

        public async Task<ApiResponse<IEnumerable<BusinessReponse>>> GetBusinessList(Guid curUserId)
        {
            try
            {
                // Get all users with BusinessOwner role
                var users = await _userManager.Users.ToListAsync();
                var businessOwners = new List<BusinessReponse>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(UserRoleEnum.BusinessOwner.ToString()))
                    {
                        // Count members managed by this business owner
                        var memberCount = await _userRepository
                            .CountManagedMembersByBO(user.Id);

                        // Count projects owned by this business owner
                        var projectCount = await _userRepository.CountProjectsOwnedByBO(user.Id);
                        // Check curUserId phải member không
                        var curUser = await _userManager.FindByIdAsync(curUserId.ToString());
                        var curUserRoles = await _userManager.GetRolesAsync(curUser);
                        var canRequestJoin = false;
                        if (curUserRoles.Contains(UserRoleEnum.Member.ToString()))
                        {
                            // Nếu là member thì tìm canRequestJoin
                            // Nếu tồn tại invitation rồi thì canRequestJoin = false
                            // Ngược lại canRequestJoin = true
                            canRequestJoin = !(await _organizationInviteRepository.IsInvitationExistsAsync(user.Id, curUserId));
                        }

                        businessOwners.Add(new BusinessReponse
                        {
                            Id = user.Id,
                            BusinessOwnerName = user.FullName,
                            BusinessName = user.Organization,
                            CreatedAt = user.CreatedAt,
                            MemberCount = memberCount,
                            ProjectCount = projectCount,
                            CanRequestJoin = canRequestJoin
                        });
                    }
                }

                return ApiResponse<IEnumerable<BusinessReponse>>.SuccessResponse(businessOwners, "Business owners retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<BusinessReponse>>.ErrorResponse(null, $"Error retrieving business owners: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BusinessReponse>> GetBusinessDetail(Guid businessOwnerId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == businessOwnerId);
            if (user == null)
            {
                return ApiResponse<BusinessReponse>.ErrorResponse(null, "Business owner not found.");
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(UserRoleEnum.BusinessOwner.ToString()))
            {
                return ApiResponse<BusinessReponse>.ErrorResponse(null, "User is not a BusinessOwner.");
            }
            // Count members managed by this business owner
            var memberCount = await _userRepository.CountManagedMembersByBO(user.Id);
            // Count projects owned by this business owner
            var projectCount = await _userRepository.CountProjectsOwnedByBO(user.Id);
            var response = new BusinessReponse
            {
                Id = user.Id,
                BusinessOwnerName = user.FullName,
                BusinessName = user.Organization,
                CreatedAt = user.CreatedAt,
                MemberCount = memberCount,
                ProjectCount = projectCount,
            };
            return ApiResponse<BusinessReponse>.SuccessResponse(response, "Business owner details retrieved successfully.");
        }

        public async Task<ApiResponse<string>> RemoveMemberFromOrganizationAsync(Guid businessOwnerId, Guid memberId)
        {
            try
            {
                // 1. Lấy member
                var member = await _userManager.FindByIdAsync(memberId.ToString());
                if (member == null)
                    return ApiResponse<string>.ErrorResponse("Member not found.");

                // 2. Kiểm tra member thuộc tổ chức của BO không
                if (member.ManagedById != businessOwnerId || string.IsNullOrEmpty(member.Organization))
                    return ApiResponse<string>.ErrorResponse("This member does not belong to your organization.");

                var oldOrganization = member.Organization;

                // 3. Set lại Organization và ManagedById = null
                member.Organization = null;
                member.ManagedById = null;

                var updateResult = await _userManager.UpdateAsync(member);
                if (!updateResult.Succeeded)
                    return ApiResponse<string>.ErrorResponse("Failed to remove member from organization.");

                // 4. Tìm các project thuộc tổ chức của BO
                var projectIds = await _projectRepository.GetProjectIdsByOwnerIdAsync(businessOwnerId);

                // 5. Lấy các ProjectMember active có member này tham gia trong các project đó
                var projectMemberships = await _projectMemberRepository.GetActiveMembershipsByMemberAndProjectsAsync(memberId, projectIds);

                // 6. Cập nhật LeftAt = now
                var now = DateTime.UtcNow;
                foreach (var pm in projectMemberships)
                {
                    pm.LeftAt = now;
                }
                await _projectMemberRepository.UpdateRangeAsync(projectMemberships);
                await _projectMemberRepository.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(
                    $"Removed member from organization and updated {projectMemberships.Count} project(s).",
                    "Member removed successfully."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error removing member from organization: {ex.Message}");
            }
        }
    }
}

