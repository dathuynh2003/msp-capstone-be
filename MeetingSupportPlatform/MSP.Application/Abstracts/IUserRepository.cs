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
        Task<IEnumerable<User>> GetMembersManagedByAsync(Guid businessOwnerId);
        Task<int> CountProjectsOwnedByBO(Guid id);
        Task<int> CountManagedMembersByBO(Guid id);
        Task<IEnumerable<User>> GetUsersWithExpiredRefreshTokensAsync(DateTime currentTime);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();

    }
}
