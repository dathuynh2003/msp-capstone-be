using MSP.Application.Abstracts;
using MSP.Domain.Entities;

namespace MSP.Application.Repositories
{
    public interface IUserDeviceRepository : IGenericRepository<UserDevice, Guid>
    {
        Task<UserDevice?> GetByFCMTokenAsync(string fcmToken);
        Task<List<UserDevice>> GetActiveDevicesByUserIdAsync(Guid userId);
        Task<bool> DeactivateDeviceAsync(Guid deviceId);
        Task<int> DeactivateAllUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
