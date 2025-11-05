using MSP.Application.Abstracts;
using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Repositories
{
    public interface ITaskReassignRequestRepository : IGenericRepository<TaskReassignRequest, Guid>
    {
        Task<TaskReassignRequest?> GetTaskReassignRequestByIdAsync(Guid id);
        Task<IEnumerable<TaskReassignRequest>> GetTaskReassignRequestsByTaskIdAsync(Guid taskId);
        Task<IEnumerable<TaskReassignRequest>> GetTaskReassignRequestsForUserAsync(Guid userId);
    }
}
