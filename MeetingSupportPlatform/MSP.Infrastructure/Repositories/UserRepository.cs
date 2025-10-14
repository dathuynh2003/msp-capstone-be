using MSP.Application.Abstracts;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using MSP.Shared.Enums;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<User?> GetUserByGoogleIdAsync(string googleId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<IEnumerable<User>> GetBusinessOwnersAsync()
        {
            return await _context.Users
                .Where(u => _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                    .Any(ur => ur.UserId == u.Id && ur.Name == UserRoleEnum.BusinessOwner.ToString()))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetPendingBusinessOwnersAsync()
        {
            return await _context.Users
                .Where(u => !u.IsApproved && _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                    .Any(ur => ur.UserId == u.Id && ur.Name == UserRoleEnum.BusinessOwner.ToString()))
                .ToListAsync();
        }
    }
}
