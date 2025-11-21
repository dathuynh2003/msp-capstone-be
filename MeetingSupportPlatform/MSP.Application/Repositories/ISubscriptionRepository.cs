using MSP.Application.Abstracts;
using MSP.Application.Services.Interfaces.Summarize;
using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Repositories
{
    public interface ISubscriptionRepository : IGenericRepository<Subscription, Guid>
    {
        Task<Subscription?> GetByOrderCodeAsync(long orderCode);
        Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Subscription>> GetAllAsync();

        Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId);
    }
}
