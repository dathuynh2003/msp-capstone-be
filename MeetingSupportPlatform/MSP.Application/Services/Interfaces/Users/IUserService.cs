using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Users
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<User>>> GetBusinessOwnersAsync();
        Task<ApiResponse<IEnumerable<User>>> GetPendingBusinessOwnersAsync();
    }
}

