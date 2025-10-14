using MSP.Application.Abstracts;
using MSP.Application.Services.Interfaces.Users;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<IEnumerable<User>>> GetBusinessOwnersAsync()
        {
            try
            {
                var businessOwners = await _userRepository.GetBusinessOwnersAsync();
                return ApiResponse<IEnumerable<User>>.SuccessResponse(businessOwners, "Business owners retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<User>>.ErrorResponse(null, $"Error retrieving business owners: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<User>>> GetPendingBusinessOwnersAsync()
        {
            try
            {
                var pendingBusinessOwners = await _userRepository.GetPendingBusinessOwnersAsync();
                return ApiResponse<IEnumerable<User>>.SuccessResponse(pendingBusinessOwners, "Pending business owners retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<User>>.ErrorResponse(null, $"Error retrieving pending business owners: {ex.Message}");
            }
        }
    }
}

