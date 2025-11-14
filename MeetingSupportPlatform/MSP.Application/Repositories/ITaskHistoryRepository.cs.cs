using MSP.Application.Abstracts;
using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Repositories
{
    public interface ITaskHistoryRepository : IGenericRepository<TaskHistory, Guid>
    {
        Task<TaskHistory?> GetTaskHistoryByIdAsync(Guid id);
        Task<IEnumerable<TaskHistory>> GetTaskHistoriesByTaskIdAsync(Guid taskId);

    }
}
