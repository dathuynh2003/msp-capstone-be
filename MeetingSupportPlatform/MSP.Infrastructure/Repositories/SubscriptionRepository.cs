using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using MSP.Shared.Enums;
using PayOS.Models.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSP.Infrastructure.Repositories
{
    public class SubscriptionRepository(ApplicationDbContext context) : GenericRepository<Subscription, Guid>(context), ISubscriptionRepository
    {
        public Task<Subscription?> GetByOrderCodeAsync(long orderCode)
        {
            return _context.Subscriptions
                .Include(s => s.Package)
                .ThenInclude(p => p.Limitations)
                .FirstOrDefaultAsync(s => s.TransactionID == orderCode.ToString());
        }
        public async Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Package)
                .ThenInclude(p => p.Limitations)
                .Include(s => s.User)
                .Where(s => s.UserId == userId && s.Status == PaymentEnum.Paid.ToString().ToUpper())
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Subscription>> GetAllAsync()
        {
            return await _context.Subscriptions
                .Include(s => s.Package)
                .ThenInclude(p => p.Limitations)
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId)
        {
            return await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Package)
                .ThenInclude(p => p.Limitations)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        }


    }
}
