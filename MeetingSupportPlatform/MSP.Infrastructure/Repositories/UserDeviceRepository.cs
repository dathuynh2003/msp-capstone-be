using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;

namespace MSP.Infrastructure.Repositories
{
    public class UserDeviceRepository(ApplicationDbContext context) : GenericRepository<UserDevice, Guid>(context), IUserDeviceRepository
    {
        public async Task<UserDevice?> GetByFCMTokenAsync(string fcmToken)
        {
            return await _context.UserDevices
                .FirstOrDefaultAsync(d => d.FCMToken == fcmToken);
        }

        public async Task<List<UserDevice>> GetActiveDevicesByUserIdAsync(Guid userId)
        {
            return await _context.UserDevices
                .AsNoTracking()
                .Where(d => d.UserId == userId && d.IsActive && !d.IsDeleted)
                .OrderByDescending(d => d.LastActiveAt)
                .ToListAsync();
        }

        public async Task<bool> DeactivateDeviceAsync(Guid deviceId)
        {
            var device = await GetByIdAsync(deviceId);
            if (device == null) return false;

            device.IsActive = false;
            device.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(device);
            return true;
        }

        public async Task<int> DeactivateAllUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var count = await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(d => d.IsActive, false)
                        .SetProperty(d => d.UpdatedAt, DateTime.UtcNow),
                    cancellationToken);

            return count;
        }

    }
}
