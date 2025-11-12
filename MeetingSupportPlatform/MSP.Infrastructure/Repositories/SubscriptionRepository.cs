using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
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
                .FirstOrDefaultAsync(s => s.TransactionID == orderCode.ToString());
        }
    }
}
