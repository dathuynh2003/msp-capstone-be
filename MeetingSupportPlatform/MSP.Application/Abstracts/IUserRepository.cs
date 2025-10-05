using MSP.Domain.Entities;

namespace MSP.Application.Abstracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<User?> GetUserByGoogleIdAsync(string googleId);
    }
}
