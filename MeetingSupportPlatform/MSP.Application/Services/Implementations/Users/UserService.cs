using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Responses.Users;
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
        public UserService(UserManager<User> userManager, IUserRepository userRepository, INotificationService notificationService)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _notificationService = notificationService;
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
                Title = "Tài khoản đã được duyệt",
                Message = $"Chào mừng {user.FullName}! Tài khoản BusinessOwner của bạn đã được admin duyệt. Bạn có thể đăng nhập vào hệ thống.",
                Type = MSP.Shared.Enums.NotificationTypeEnum.InApp.ToString(),
                Data = $"{{\"eventType\":\"AccountApproved\",\"userId\":\"{user.Id}\"}}"
            });


            // Gửi email thông báo BusinessOwner được duyệt
            var emailBody = EmailNotificationTemplate.BusinessOwnerStatusNotification(user.FullName, true);

            _notificationService.SendEmailNotification(
                user.Email,
                "Tài khoản BusinessOwner đã được duyệt - Meeting Support Platform",
                emailBody
            );

            return ApiResponse<string>.SuccessResponse("BusinessOwner approved successfully!", "Chấp nhận người dùng làm BusinessOwner thành công.");
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
                Title = "Tài khoản bị từ chối",
                Message = $"Xin chào {user.FullName}, tài khoản BusinessOwner của bạn đã bị từ chối. Vui lòng liên hệ admin để biết thêm chi tiết.",
                Type = MSP.Shared.Enums.NotificationTypeEnum.InApp.ToString(),
                Data = $"{{\"eventType\":\"AccountRejected\",\"userId\":\"{user.Id}\"}}"
            });


            // Gửi email thông báo BusinessOwner bị từ chối
            var emailBody = EmailNotificationTemplate.BusinessOwnerStatusNotification(user.FullName, false);

            _notificationService.SendEmailNotification(
                user.Email,
                "Yêu cầu BusinessOwner không được chấp nhận - Meeting Support Platform",
                emailBody
            );

            return ApiResponse<string>.SuccessResponse("BusinessOwner rejected successfully!", "Từ chối người dùng làm BusinessOwner thành công.");
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
            var vnStatus = user.IsActive ? "Kích hoạt người dùng thành công." : "Vô hiệu hóa người dùng thành công.";

            return ApiResponse<string>.SuccessResponse($"User {status} successfully!", vnStatus);
        }

        public async Task<ApiResponse<IEnumerable<GetUserResponse>>> GetMembersManagedByAsync(Guid businessOwnerId)
        {
            var businessOwner = await _userManager.FindByIdAsync(businessOwnerId.ToString());
            var members = await _userRepository.GetMembersManagedByAsync(businessOwnerId);
            var result = new List<GetUserResponse>();
            foreach (var user in members)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Member";
                result.Add(new GetUserResponse
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
                    RoleName = role
                });
            }
            return ApiResponse<IEnumerable<GetUserResponse>>.SuccessResponse(result, "Members retrieved successfully.");

        }
    }
}

