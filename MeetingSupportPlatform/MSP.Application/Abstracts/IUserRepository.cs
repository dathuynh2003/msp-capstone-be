using MSP.Domain.Entities;
using MSP.Shared.Enums;

namespace MSP.Application.Abstracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<User?> GetUserByGoogleIdAsync(string googleId);
        Task<IEnumerable<User>> GetBusinessOwnersAsync();
        Task<IEnumerable<User>> GetPendingBusinessOwnersAsync();
    }
}
