using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Infrastructure.Repositories
{
    public class TaskReassignRequestRepository(ApplicationDbContext context) : GenericRepository<TaskReassignRequest, Guid>(context), ITaskReassignRequestRepository
    {
        public Task<TaskReassignRequest?> GetTaskReassignRequestByIdAsync(Guid id)
        {
            return _context.TaskReassignRequests
                .Include(t => t.Task)
                .Include(t => t.FromUser)
                .Include(t => t.ToUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskReassignRequest>> GetTaskReassignRequestsByTaskIdAsync(Guid taskId)
        {
            return await _context.TaskReassignRequests
                .Include(r => r.Task)
                .Include(r => r.FromUser)
                .Include(r => r.ToUser)
                .Where(r => r.TaskId == taskId && !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskReassignRequest>> GetTaskReassignRequestsForUserAsync(Guid userId)
        {
            return await _context.TaskReassignRequests
                .Include(r => r.Task)
                .Include(r => r.FromUser)
                .Include(r => r.ToUser)
                .Where(r => (r.FromUserId == userId || r.ToUserId == userId) && !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
