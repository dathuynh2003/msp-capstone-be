using MSP.Application.Models.Requests.User;
using MSP.Application.Models.Responses.Users;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Users
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserResponse>>> GetBusinessOwnersAsync();
        Task<ApiResponse<IEnumerable<UserResponse>>> GetPendingBusinessOwnersAsync();

        Task<ApiResponse<string>> ApproveBusinessOwnerAsync(Guid userId);
        Task<ApiResponse<string>> RejectBusinessOwnerAsync(Guid userId);
        Task<ApiResponse<string>> ToggleUserActiveStatusAsync(Guid userId);
        Task<ApiResponse<IEnumerable<GetUserResponse>>> GetMembersManagedByAsync(Guid businessOwnerId);
        Task<ApiResponse<ReAssignRoleResponse>> ReAssignRoleAsync(ReAssignRoleRequest request);
        Task<ApiResponse<UserDetailResponse>> GetUserDetailByIdAsync(Guid userId);
        Task<ApiResponse<IEnumerable<BusinessReponse>>> GetBusinessList(Guid curUserId);
        Task<ApiResponse<BusinessReponse>> GetBusinessDetail(Guid businessOwnerId);
        Task<ApiResponse<string>> RemoveMemberFromOrganizationAsync(Guid businessOwnerId, Guid memberId);

        Task<ApiResponse<string>> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);

    }
}

